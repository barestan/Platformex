using System;

namespace Platformex.Domain
{
    public static class StreamHelper 
    {
        public static string EventStreamName(Type eventType, bool isSync) 
            => $"{(isSync ? "sync=:" : "async:")} {eventType.FullName}";

    }
}