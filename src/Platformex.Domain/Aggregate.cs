
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;

[assembly:InternalsVisibleTo("Platformex.Infrastructure")]

namespace Platformex.Domain
{
    public abstract class Aggregate<TIdentity, TState> : Grain, IAggregate<TIdentity>
        where TIdentity : Identity<TIdentity>
        where TState : IAggregateState<TIdentity>
    {
        public TIdentity AggregateId => State?.Id ?? this.GetId<TIdentity>();
        protected TState State { get; private set;}

        private IPlatform _platform;

        private ILogger _logger;
        protected ILogger Logger => GetLogger();

        private ILogger GetLogger() 
            => _logger ??= ServiceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());

        protected virtual string GetAggregateName() => GetType().Name.Replace("Aggregate", "");
        protected string GetPrettyName() => $"{GetAggregateName()}:{this.GetPrimaryKeyString()}";

        public override async Task OnActivateAsync()
        {
            Logger.LogInformation($"Aggregate [{GetPrettyName()}] activating...");
            try
            {
                _platform = (IPlatform) this.ServiceProvider.GetService(typeof(IPlatform));

                var stateType = _platform.Definitions.Aggregate<TIdentity>()?.StateType;

                if (stateType == null)
                    throw new Exception($"Definitions on aggregate {typeof(TIdentity).Name} not found");

                Logger.LogInformation($"Aggregate [{GetPrettyName()}] state loading...");
                State = (TState) Activator.CreateInstance(stateType);
                await State.LoadState(this.GetId<TIdentity>());
                Logger.LogInformation($"Aggregate [{GetPrettyName()}] state loaded.");

                await base.OnActivateAsync();
            }
            catch (Exception e)
            {
                Logger.LogError($"Aggregate [{GetPrettyName()}] activation error: {e.Message}", e);
                throw;
            }
            Logger.LogInformation($"Aggregate [{GetPrettyName()}] activated.");
        }

        public override Task OnDeactivateAsync()
        {
            Logger.LogInformation($"Aggregate [{GetPrettyName()}] activated.");
            return base.OnDeactivateAsync();
        }

        protected async Task Emit<TEvent>(TEvent e) where TEvent : class, IAggregateEvent<TIdentity>
        {
            Logger.LogInformation($"Aggregate [{GetPrettyName()}] preparing to emit event {e.GetPrettyName()}...");
            var domainEvent = new DomainEvent<TIdentity, TEvent>(e.Id, e, DateTimeOffset.Now, 1);
            try
            {
                Logger.LogInformation($"Aggregate [{GetPrettyName()}] changes state ...");
                await State.Apply(e);
                Logger.LogInformation($"Aggregate [{GetPrettyName()}] changed state ...");

                Logger.LogInformation($"Aggregate [{GetPrettyName()}] fires event {e.GetPrettyName()}...");

                //Посылаем сообщения асинхронно
                var _ = GetStreamProvider("EventBusProvider")
                    .GetStream<IDomainEvent>(Guid.Empty, StreamHelper.EventStreamName(typeof(TEvent),false))
                    .OnNextAsync(domainEvent).ConfigureAwait(false);

                //Посылаем сообщения синхронно
                await GetStreamProvider("EventBusProvider")
                    .GetStream<IDomainEvent>(Guid.Empty, StreamHelper.EventStreamName(typeof(TEvent), true))
                    .OnNextAsync(domainEvent).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.LogInformation($"Aggregate [{GetPrettyName()}] error fires event: {domainEvent.GetPrettyName()} : {ex.Message}", e);
                throw;
            }
            Logger.LogInformation($"Aggregate [{GetPrettyName()}] fired event {e.GetPrettyName()}...");
        }

    }
}