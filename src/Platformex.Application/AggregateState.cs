using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Platformex.Domain;

namespace Platformex.Application
{
    public abstract class AggregateState<TIdentity, TEventApplier> :
        IAggregateState<TIdentity> where TIdentity : Identity<TIdentity>
    {
        private static readonly IReadOnlyDictionary<Type, Action<TEventApplier, IAggregateEvent>> ApplyMethods; 

        static AggregateState()
        {
            ApplyMethods = typeof(TEventApplier).GetAggregateEventApplyMethods<TIdentity, TEventApplier>();
        }
        public TIdentity Id { get; protected set; }

        public virtual Task LoadState(TIdentity id)
        {
            Id = id;
            return Task.CompletedTask;
        }

        public async Task Apply(IAggregateEvent<TIdentity> e)
        {
            await BeforeApply(e);
            
            var aggregateEventType = e.GetType();
            Action<TEventApplier, IAggregateEvent> applier;

            if (!ApplyMethods.TryGetValue(aggregateEventType, out applier))
            {
                throw new MissingMethodException($"missing Apply({aggregateEventType.Name})");
            }

            applier((TEventApplier) (object) this, e);
            
            await AfterApply(e);
        }

        protected virtual Task BeforeApply(IAggregateEvent<TIdentity> id) => Task.CompletedTask;

        protected  virtual Task AfterApply(IAggregateEvent<TIdentity> id) => Task.CompletedTask;

    }
}