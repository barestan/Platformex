using System;

namespace Platformex.Domain
{
    public class DomainEvent<TIdentity, TAggregateEvent> : IDomainEvent<TIdentity, TAggregateEvent>
        where TIdentity : Identity<TIdentity>
        where TAggregateEvent : class, IAggregateEvent<TIdentity>
    {
        public Type IdentityType => typeof(TIdentity);
        public Type EventType => typeof(TAggregateEvent);

        public TIdentity AggregateIdentity { get; }
        public TAggregateEvent AggregateEvent { get; }
        public Int64 AggregateSequenceNumber { get; }

        // ReSharper disable once UnassignedGetOnlyAutoProperty
        public IEventMetadata Metadata { get; }

        public DateTimeOffset Timestamp { get; }

        public DomainEvent(
            TIdentity aggregateIdentity,
            TAggregateEvent aggregateEvent,
            DateTimeOffset timestamp,
            Int64 aggregateSequenceNumber)
        {
            if (timestamp == default) throw new ArgumentNullException(nameof(timestamp));
            if (aggregateIdentity == null || string.IsNullOrEmpty(aggregateIdentity.Value)) throw new ArgumentNullException(nameof(aggregateIdentity));
            if (aggregateSequenceNumber <= 0) throw new ArgumentOutOfRangeException(nameof(aggregateSequenceNumber));

            AggregateEvent = aggregateEvent ?? throw new ArgumentNullException(nameof(aggregateEvent));
            Timestamp = timestamp;
            AggregateIdentity = aggregateIdentity;
            AggregateSequenceNumber = aggregateSequenceNumber;
        }

        public IIdentity GetIdentity() => AggregateIdentity;

        public IAggregateEvent GetAggregateEvent() => AggregateEvent;

        public override string ToString() => $"{IdentityType} v{AggregateSequenceNumber}/{EventType}:{AggregateIdentity}";
    }
}