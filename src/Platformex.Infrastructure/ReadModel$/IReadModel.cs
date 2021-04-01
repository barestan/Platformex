using System.Threading.Tasks;
using Orleans;

namespace Platformex.Infrastructure
{
    public interface IReadModel : IGrainWithStringKey
    {
        Task ProcessEvent(IDomainEvent e);
    }
}