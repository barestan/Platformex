using Orleans;

namespace Platformex
{
    public interface IAggregate : IGrainWithStringKey
    {
    }

    public interface IAggregate<out T> : IAggregate where T : Identity<T>
    {
    }
}