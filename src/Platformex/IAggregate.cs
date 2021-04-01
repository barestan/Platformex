using Orleans;

namespace Platformex
{
    public interface IAggregate<out T> : IGrainWithStringKey where T : Identity<T>
    {
    }
}