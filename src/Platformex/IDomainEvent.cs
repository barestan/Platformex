using System;

namespace Platformex
{
    public interface IDomainEvent
    {
        Type IdentityType { get; }
        Type EventType { get; }
        long AggregateSequenceNumber { get; }
        //IEventMetadata Metadata { get; }
        DateTimeOffset Timestamp { get; }

        IIdentity GetIdentity();
        IAggregateEvent GetAggregateEvent();
    }

    public interface IDomainEvent<out TIdentity> : IDomainEvent where TIdentity : IIdentity
    {
        TIdentity AggregateIdentity { get; }
    }

    public interface IDomainEvent<out TIdentity, out TAggregateEvent> : IDomainEvent<TIdentity> where TIdentity : Identity<TIdentity>
        where TAggregateEvent : class, IAggregateEvent<TIdentity>
    {
        TAggregateEvent AggregateEvent { get; }
    }
}
