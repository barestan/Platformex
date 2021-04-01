using System;
using System.Collections.Generic;

namespace Platformex
{
    public interface IEventMetadata : ICommonMetadata
    {
        IEventId EventId { get; }
        string EventName { get; }
        int EventVersion { get; }
        DateTimeOffset Timestamp { get; }
        long TimestampEpoch { get; }
        long AggregateSequenceNumber { get; }
        string AggregateId { get; }
        string CausationId { get; }

        IEventMetadata CloneWith(params KeyValuePair<string, string>[] keyValuePairs);
        IEventMetadata CloneWith(IEnumerable<KeyValuePair<string, string>> keyValuePairs);
    }
    public interface IEventId : ISourceId
    {
    }
}