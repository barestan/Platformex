using System.Threading.Tasks;
using Orleans;

namespace Platformex.Application
{
    public interface IQueryHandler<TResult> : IGrainWithStringKey
    {
        Task<TResult> QueryAsync(IQuery<TResult> query);
    }
}
