using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Concurrency;
using Orleans.Streams;

namespace Platformex.Domain
{
    public interface ISaga : IGrainWithStringKey
    {
        Task ProcessEvent(IDomainEvent e);
    } 

    public interface IStartedBy<in TIdentity, in TAggregateEvent> 
        where TAggregateEvent : class, IAggregateEvent<TIdentity>
        where TIdentity : Identity<TIdentity>
    {
        Task<string> HandleAsync(IDomainEvent<TIdentity, TAggregateEvent> domainEvent);
    }
    public interface IStartedBySync<in TIdentity, in TAggregateEvent> : IStartedBy<TIdentity, TAggregateEvent> 
        where TAggregateEvent : class, IAggregateEvent<TIdentity>
        where TIdentity : Identity<TIdentity>
    {

    }
    [Reentrant]
    public abstract class Saga<TSaga> : Grain, ISaga, IDomain
        where TSaga : Saga<TSaga> 
    {
        private static readonly IReadOnlyDictionary<Type, Func<TSaga, IDomainEvent, Task>> ApplyMethods = typeof(TSaga)
            .GetReadModelEventApplyMethods<TSaga>();
        
        private static readonly IReadOnlyList<Tuple<Type, Type, bool>> AsyncSubscriptionTypes = 
            typeof(TSaga).GetSubscribersTypes(false);

        private static readonly IReadOnlyList<Tuple<Type, Type, bool>> SyncSubscriptionTypes = 
            typeof(TSaga).GetSubscribersTypes(true);

        // ReSharper disable once StaticMemberInGenericType
        private static readonly IReadOnlyList<Type> StartedEventTypes =
            AsyncSubscriptionTypes.Where(i => i.Item3).Select(i => i.Item2).Concat(
                SyncSubscriptionTypes.Where(i => i.Item3).Select(i => i.Item2)).ToList();

        private ILogger _logger;
        protected ILogger Logger => GetLogger();
        private ILogger GetLogger() 
            => _logger ??= ServiceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());

        private IDisposable _timer;

        protected IDomain Domain => this;

        protected virtual string GetSagaId(IDomainEvent startDomainEvent) => startDomainEvent.GetIdentity().Value;
        protected virtual Task<bool> LoadStateAsync() => Task.FromResult(false);
        protected virtual Task SaveStateAsync() => Task.CompletedTask;

        protected virtual string GetPrettyName() => $"{GetSagaName()}:{this.GetPrimaryKeyString()}";
        protected virtual string GetSagaName() => GetType().Name.Replace("Saga", "");

        private bool _isStarted;
        public virtual async Task ProcessEvent(IDomainEvent e)
        {
            Logger.LogInformation($"(Saga [{GetPrettyName()}] received event {e.GetPrettyName()}.");
            
            //Проверяем, является ли он стартовым 
            if (!StartedEventTypes.Contains(e.EventType) && !_isStarted)
            {
                Logger.LogWarning( $"(Saga [{GetPrettyName()}] event {e.GetPrettyName()} is not start-event.");
                return; //Игнорируем 
            }
            _isStarted = true;

            var applyMethod = GetEventApplyMethods(e);
            await applyMethod(e);


            Logger.LogInformation($"(Saga [{GetPrettyName()}] handled event {e.GetPrettyName()}.");
        }

        private Func<IDomainEvent, Task> GetEventApplyMethods(IDomainEvent aggregateEvent)
        {
            var eventType = aggregateEvent.GetType();
            var method = ApplyMethods.FirstOrDefault(i => i.Key.IsAssignableFrom(eventType)).Value;

            if (method == null)
                throw new NotImplementedException($"Saga of Type={GetType()} does not have an 'HandleAsync' method that takes in an aggregate event of Type={eventType} as an argument.");

            var aggregateApplyMethod = method.Bind(this as TSaga);

            return aggregateApplyMethod;
        }

        public sealed override async Task OnActivateAsync()
        {
            //Это корневой менеджер
            bool isManager = this.GetPrimaryKeyString() == null;

            Logger.LogInformation(isManager
                ? $"(Saga [{GetPrettyName()}] activating..."
                : $"(Saga Manager [{GetSagaName()}] activating...");
            try
            {
                var streamProvider = GetStreamProvider("EventBusProvider");
                
                //Игнорируем инициализирующее событие 
                await streamProvider.GetStream<string>(Guid.Empty, "InitializeSubscriptions")
                    .SubscribeAsync((_, _) => Task.CompletedTask);

                if (isManager) 
                {
                    NoDeactivateRoot();

                    foreach (var subscriptionType in AsyncSubscriptionTypes)
                    {
                        var eventStream = streamProvider.GetStream<IDomainEvent>(Guid.Empty, 
                                StreamHelper.EventStreamName(subscriptionType.Item2,false));

                        await SubscribeAndProcess(eventStream, false);
                    }
                   
                    foreach (var subscriptionType in SyncSubscriptionTypes)
                    {
                        var eventStream = streamProvider.GetStream<IDomainEvent>(Guid.Empty, 
                            StreamHelper.EventStreamName(subscriptionType.Item2,true));

                        await SubscribeAndProcess(eventStream, true);
                    }
                }
                else
                {
                    //Загружаем состояние саги
                    _isStarted = await LoadStateAsync();
                }
            }
            catch (Exception e)
            {
                Logger.LogInformation(isManager
                    ? $"(Saga [{GetPrettyName()}] activation error: {e.Message}"
                    : $"(Saga Manager [{GetSagaName()}] activation error: {e.Message}", e);
                throw;
            }
            Logger.LogInformation(isManager
                ? $"(Saga [{GetPrettyName()}] activated."
                : $"(Saga [{GetSagaName()}] activated...");
        }
        private async Task SubscribeAndProcess(IAsyncStream<IDomainEvent> eventStream, bool isSync)
        {
            //Подписываемся на события
            await eventStream.SubscribeAsync(async (data, _) =>
            {
                Logger.LogInformation($"(Saga Manager [{GetSagaName()}] received event {data.GetPrettyName()}.");

                
                //Определяем ID саги
                var sagaId = GetSagaId(data);

                var saga = GrainFactory.GetGrain<ISaga>(sagaId, GetType().FullName);
                
                Logger.LogInformation(
                    $"(Saga Manager [{GetSagaName()}] send event to Saga {data.GetPrettyName()}.");
                //Вызываем сагу для обработки события
                if (isSync)
                {
                    await saga.ProcessEvent(data).ConfigureAwait(false);
                }
                else
                {
                    var __ = saga.ProcessEvent(data).ConfigureAwait(false);
                }

            });
        }
        public override Task OnDeactivateAsync()
        {
            Logger.LogInformation(this.GetPrimaryKeyString() == null
                ? $"(Saga [{GetPrettyName()}] deactivated."
                : $"(Saga Manager [{GetSagaName()}] deactivated...");
            return base.OnDeactivateAsync();
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

        public TAggregate GetAggregate<TAggregate>(string id) where TAggregate : IAggregate => GrainFactory.GetGrain<TAggregate>(id);
    }
}
