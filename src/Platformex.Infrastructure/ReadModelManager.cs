using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleans;
using Orleans.Concurrency;
using Orleans.Streams;

namespace Platformex.Infrastructure
{
    public interface IReadModel : IGrainWithStringKey
    {
        Task ProcessEvent(IDomainEvent e);
    }
    public interface IAmReadModelFor<in TIdentity, in TAggregateEvent>
        where TIdentity : Identity<TIdentity>
        where TAggregateEvent : class, IAggregateEvent<TIdentity>
    {
        Task Apply(IDomainEvent<TIdentity, TAggregateEvent> domainEvent);
    }

    [Reentrant]
    public abstract class ReadModel<TReadModel> : Grain, IReadModel where TReadModel : class, IReadModel
    {
        private static readonly IReadOnlyDictionary<Type, Func<TReadModel, IDomainEvent, Task>> ApplyMethods = typeof(TReadModel)
            .GetReadModelEventApplyMethods<TReadModel>();
        
        private IDisposable _timer;

        protected virtual Type GetIdenitityType() => null;

        public sealed override async Task OnActivateAsync()
        {
            var streamProvider = GetStreamProvider("EventBusProvider");
            var initStream = streamProvider.GetStream<string>(Guid.Empty, "InitializeSubscriptions");
            await initStream.SubscribeAsync((_, _) => Task.CompletedTask);
            if (GetType() == typeof(IReadModel)) DeactivateRoot();
            else NoDeactivateRoot();

            if (this.GetPrimaryKeyString() == null) //Это корневой менеджер
            {
                //Это фильтр на тип агрегата (null - без фильтра)
                var identytyType = GetIdenitityType();
                
                var subscriptionTypes =
                    typeof(TReadModel).GetReadModelSubscribersTypes().Where(i=> identytyType == null || identytyType == i.Item1);

                foreach (var subscriptionType in subscriptionTypes)
                {
                    var eventStream = streamProvider.GetStream<IDomainEvent>(Guid.Empty, subscriptionType.Item2.FullName);

                    await eventStream.SubscribeAsync((data, _) =>
                    {
                        var readModelId = GetReadModelId(data);
                        var manager = GrainFactory.GetGrain<IReadModel>(readModelId, GetType().FullName);

                        manager.ProcessEvent(data);
                        return Task.CompletedTask;
                    });
                }
            }
        }

        protected abstract string GetReadModelId(IDomainEvent domainEvent);

        public virtual async Task ProcessEvent(IDomainEvent e)
        {
            var applyMethods = GetEventApplyMethods(e);
            await applyMethods(e);
        }

        protected Func<IDomainEvent, Task> GetEventApplyMethods(IDomainEvent aggregateEvent)
        {
            var eventType = aggregateEvent.GetType();
            var method = ApplyMethods.FirstOrDefault(i => i.Key.IsAssignableFrom(eventType)).Value;

            if (method == null)
                throw new NotImplementedException($"ReadModel of Type={GetType()} does not have an 'Apply' method that takes in an aggregate event of Type={eventType} as an argument.");

            var aggregateApplyMethod = method.Bind(this as TReadModel);

            return aggregateApplyMethod;
        }
        private void DeactivateRoot()
        {
            _timer?.Dispose();
            _timer = RegisterTimer(_ =>
            {
                DeactivateOnIdle();

                _timer?.Dispose();
                _timer = null;

                return Task.CompletedTask;
            }, null, TimeSpan.FromMilliseconds(10), TimeSpan.MaxValue);
        }

        private void NoDeactivateRoot()
        {
            _timer?.Dispose();
            _timer = RegisterTimer(_ =>
            {
                var key = this.GetPrimaryKeyString();
                
                if (key == null) 
                    DelayDeactivation(TimeSpan.FromDays(100));

                _timer?.Dispose();
                _timer = null;

                return Task.CompletedTask;
            }, null, TimeSpan.FromMilliseconds(10), TimeSpan.MaxValue);
        }

    }

    public abstract class AggregateReadModel<TIdentity, TReadModel> : ReadModel<TReadModel> 
        where TIdentity : IIdentity
        where TReadModel : class, IReadModel
    {
        protected string Id => this.GetPrimaryKeyString();
        protected override Type GetIdenitityType() => typeof(TIdentity);

        protected override string GetReadModelId(IDomainEvent domainEvent) => domainEvent.GetIdentity().Value;
    }

    public class EventSubscriber : ImplicitStreamSubscriptionAttribute
    {
        public EventSubscriber() : base("InitializeSubscriptions")
        {

        }
    }
}
