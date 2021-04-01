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
        //public IEventMetadata Metadata { get; }
        public DateTimeOffset Timestamp { get; }

        public DomainEvent(
            TIdentity aggregateIdentity,
            TAggregateEvent aggregateEvent,
            DateTimeOffset timestamp,
            Int64 aggregateSequenceNumber)
        {
            if (aggregateEvent == null) throw new ArgumentNullException(nameof(aggregateEvent));
            if (timestamp == default) throw new ArgumentNullException(nameof(timestamp));
            if (aggregateIdentity == null || String.IsNullOrEmpty(aggregateIdentity.Value)) throw new ArgumentNullException(nameof(aggregateIdentity));
            if (aggregateSequenceNumber <= 0) throw new ArgumentOutOfRangeException(nameof(aggregateSequenceNumber));

            AggregateEvent = aggregateEvent;
            Timestamp = timestamp;
            AggregateIdentity = aggregateIdentity;
            AggregateSequenceNumber = aggregateSequenceNumber;
        }

        public IIdentity GetIdentity()
        {
            return AggregateIdentity;
        }

        public IAggregateEvent GetAggregateEvent()
        {
            return AggregateEvent;
        }

        public override String ToString()
        {
            return $"{IdentityType} v{AggregateSequenceNumber}/{EventType}:{AggregateIdentity}";
        }
    }
}