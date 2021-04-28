using System.Threading.Tasks;

namespace Platformex.Domain
{
    public interface ISubscribeTo<in TIdentity, in TAggregateEvent>
        where TIdentity : Identity<TIdentity>
        where TAggregateEvent : class, IAggregateEvent<TIdentity>
    {
        Task HandleAsync(IDomainEvent<TIdentity, TAggregateEvent> domainEvent);
    }

    public interface ISubscribeSyncTo<in TIdentity, in TAggregateEvent>
        where TIdentity : Identity<TIdentity>
        where TAggregateEvent : class, IAggregateEvent<TIdentity>
    {
        Task HandleAsync(IDomainEvent<TIdentity, TAggregateEvent> domainEvent);
    }
}