using System;
using System.Collections.Generic;

namespace Platformex
{
    public interface ICommonMetadata : IMetadataContainer
    {
        ISourceId SourceId { get; }
        string CorrelationId { get; }
        IReadOnlyCollection<string> CorrelationIds { get; }
    }

    public interface IMetadataContainer : IReadOnlyDictionary<string, string>
    {
        string UserId { get; }
        string GetMetadataValue(string key);
        T GetMetadataValue<T>(string key, Func<string, T> converter);
    }

    public interface ISourceId : IIdentity
    {
    }
}