using System.Threading.Tasks;

namespace Platformex.Domain
{
    public interface IAggregateState<TIdentity> where TIdentity : Identity<TIdentity>
    {
        TIdentity Id {get;}
       
        Task LoadState(TIdentity id);
        Task Apply(IAggregateEvent<TIdentity> e);
    }
}