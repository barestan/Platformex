using System.Threading.Tasks;
using Orleans;

namespace Platformex.Application
{
    public abstract class QueryHandler<TQuery, TResult> : Grain, IQueryHandler<TResult>
    where TQuery : IQuery<TResult>
    {
        public Task<TResult> QueryAsync(IQuery<TResult> query)
        {
            return ExecuteAsync((TQuery) query);
        }

        protected abstract Task<TResult> ExecuteAsync(TQuery query);
    }
}
