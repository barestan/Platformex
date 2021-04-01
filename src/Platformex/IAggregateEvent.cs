namespace Platformex
{
    public interface IAggregateEvent
    {
    }

    public interface IAggregateEvent<out T> : IAggregateEvent where T : Identity<T>
    {
        T Id { get; }
    }

    
}