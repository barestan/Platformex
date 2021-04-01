using System.Threading.Tasks;

namespace Platformex.Infrastructure
{
    public interface IAmReadModelFor<in TIdentity, in TAggregateEvent>
        where TIdentity : Identity<TIdentity>
        where TAggregateEvent : class, IAggregateEvent<TIdentity>
    {
        Task Apply(IDomainEvent<TIdentity, TAggregateEvent> domainEvent);
    }

    public interface IAmSyncReadModelFor<in TIdentity, in TAggregateEvent>
        where TIdentity : Identity<TIdentity>
        where TAggregateEvent : class, IAggregateEvent<TIdentity>
    {
        Task Apply(IDomainEvent<TIdentity, TAggregateEvent> domainEvent);
    }
}