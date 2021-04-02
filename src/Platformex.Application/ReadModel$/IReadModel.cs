using System.Threading.Tasks;
using Orleans;

namespace Platformex.Application
{
    public interface IReadModel : IGrainWithStringKey
    {
        Task ProcessEvent(IDomainEvent e);
    }
}