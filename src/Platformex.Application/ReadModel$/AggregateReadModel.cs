using System;
using Orleans;

namespace Platformex.Application
{
    public abstract class AggregateReadModel<TIdentity, TReadModel> : ReadModel<TReadModel> 
        where TIdentity : IIdentity
        where TReadModel : class, IReadModel
    {
        protected string Id => this.GetPrimaryKeyString();
        protected override Type GetIdenitityType() => typeof(TIdentity);

        protected override string GetReadModelId(IDomainEvent domainEvent) => domainEvent.GetIdentity().Value;
    }
}