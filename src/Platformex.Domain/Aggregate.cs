
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Orleans;

[assembly:InternalsVisibleTo("Platformex.Infrastructure")]

namespace Platformex.Domain
{
    public abstract class Aggregate<TIdentity, TState> : Grain, IAggregate<TIdentity>
        where TIdentity : Identity<TIdentity>
        where TState : IAggregateState<TIdentity>
    {
        protected TState State { get; private set;}
        public TIdentity Id => State.Id;
        private IPlatform _platform;

        public override async Task OnActivateAsync()
        {
            _platform = (IPlatform)this.ServiceProvider.GetService(typeof(IPlatform));

            var stateType = _platform.Definitions.Aggregate<TIdentity>()?.StateType;
            
            if (stateType == null) throw new Exception($"Definitions on aggregate {typeof(TIdentity).Name} not found");
            
            State = (TState) Activator.CreateInstance(stateType);
            await State.LoadState(this.GetId<TIdentity>());

            await base.OnActivateAsync();
        }

        protected async Task Emit<TEvent>(TEvent e) where TEvent : class, IAggregateEvent<TIdentity>
        {
            await State.Apply(e);

            var domainEvent = new DomainEvent<TIdentity, TEvent>(e.Id,e, DateTimeOffset.Now,1);

            await GetStreamProvider("EventBusProvider")
                .GetStream<IDomainEvent>(Guid.Empty,typeof(TEvent).FullName)
                .OnNextAsync(domainEvent).ConfigureAwait(false);
        }


    }


}