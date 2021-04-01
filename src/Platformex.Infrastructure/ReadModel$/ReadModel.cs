using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Concurrency;
using Orleans.Streams;
using Platformex.Domain;

namespace Platformex.Infrastructure
{
    [Reentrant]
    public abstract class ReadModel<TReadModel> : Grain, IReadModel where TReadModel : class, IReadModel
    {
        private static readonly IReadOnlyDictionary<Type, Func<TReadModel, IDomainEvent, Task>> ApplyMethods = typeof(TReadModel)
            .GetReadModelEventApplyMethods<TReadModel>();
        
        private ILogger _logger;
        protected ILogger Logger => GetLogger();

        private ILogger GetLogger() 
            => _logger ??= ServiceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());

        private IDisposable _timer;

        protected virtual Type GetIdenitityType() => null;
        protected virtual string GetReadModelName() => GetType().Name.Replace("ReadModel", "");
        protected string GetPrettyName() => $"{GetReadModelName()}:{this.GetPrimaryKeyString()}";

        public sealed override async Task OnActivateAsync()
        {
            //Это корневой менеджер
            bool isManager = this.GetPrimaryKeyString() == null;

            Logger.LogInformation(isManager
                ? $"(Read Model [{GetPrettyName()}] activating..."
                : $"(Read Model Manager [{GetReadModelName()}] activating...");
            try
            {
                var streamProvider = GetStreamProvider("EventBusProvider");
                
                //Игнорируем инициализирующее событие 
                await streamProvider.GetStream<string>(Guid.Empty, "InitializeSubscriptions")
                    .SubscribeAsync((_, _) => Task.CompletedTask);

                if (isManager) 
                {
                    NoDeactivateRoot();
                    //Это фильтр на тип агрегата (null - без фильтра)
                    var identytyType = GetIdenitityType();


                    //Асинхронные подписки
                    var asyncSubscriptionTypes =
                        typeof(TReadModel).GetReadModelSubscribersTypes(false)
                            .Where(i => identytyType == null || identytyType == i.Item1);

                    foreach (var subscriptionType in asyncSubscriptionTypes)
                    {
                        var eventStream = streamProvider.GetStream<IDomainEvent>(Guid.Empty, 
                                StreamHelper.EventStreamName(subscriptionType.Item2,false));

                        await SubscribeAndProcess(eventStream, false);
                    }

                    //Синхронные подписки
                    var syncSubscriptionTypes =
                        typeof(TReadModel).GetReadModelSubscribersTypes(true)
                            .Where(i => identytyType == null || identytyType == i.Item1);
                    
                    foreach (var subscriptionType in syncSubscriptionTypes)
                    {
                        var eventStream = streamProvider.GetStream<IDomainEvent>(Guid.Empty, 
                            StreamHelper.EventStreamName(subscriptionType.Item2,true));

                        await SubscribeAndProcess(eventStream, true);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogInformation(isManager
                    ? $"(Read Model [{GetPrettyName()}] activation error: {e.Message}"
                    : $"(Read Model Manager [{GetReadModelName()}] activation error: {e.Message}", e);
                throw;
            }
            Logger.LogInformation(isManager
                ? $"(Read Model [{GetPrettyName()}] activated."
                : $"(Read Model Manager [{GetReadModelName()}] activated...");
        }

        private async Task SubscribeAndProcess(IAsyncStream<IDomainEvent> eventStream, bool isSync)
        {
            //Подписываемся на события
            await eventStream.SubscribeAsync(async (data, _) =>
            {
                Logger.LogInformation($"(Read Model Manager [{GetReadModelName()}] received event {data.GetPrettyName()}.");
                //Получаем ID read model
                var readModelId = GetReadModelId(data);

                Logger.LogInformation(
                    $"(Read Model Manager [{GetReadModelName()}] send event to ReadModel {data.GetPrettyName()}.");
                //Вызываем read model для обработки события
                if (isSync)
                {
                    await GrainFactory.GetGrain<IReadModel>(readModelId, GetType().FullName)
                        .ProcessEvent(data).ConfigureAwait(false);
                }
                else
                {
                    var task = GrainFactory.GetGrain<IReadModel>(readModelId, GetType().FullName)
                        .ProcessEvent(data).ConfigureAwait(false);
                    
                }

            });
        }

        protected abstract string GetReadModelId(IDomainEvent domainEvent);

        public virtual async Task ProcessEvent(IDomainEvent e)
        {
            Logger.LogInformation($"(Read Model [{GetPrettyName()}] received event {e.GetPrettyName()}.");
            var applyMethods = GetEventApplyMethods(e);
            await applyMethods(e);
            Logger.LogInformation($"(Read Model [{GetPrettyName()}] handled event {e.GetPrettyName()}.");
        }
        public override Task OnDeactivateAsync()
        {
            Logger.LogInformation(this.GetPrimaryKeyString() == null
                ? $"(Read Model [{GetPrettyName()}] deactivated."
                : $"(Read Model Manager [{GetReadModelName()}] deactivated...");
            return base.OnDeactivateAsync();
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
}