namespace Platformex.Application
{
    public interface ICanApply<in TEvent, TIdentity> 
        where TEvent : IAggregateEvent<TIdentity> 
        where TIdentity : Identity<TIdentity>
    {
        void Apply(TEvent @event);
    }
}