using System.Threading.Tasks;

namespace Platformex
{
    public interface IDomain
    {
        TAggregate GetAggregate<TAggregate>(string id)
            where TAggregate : IAggregate;

    }
    public interface IPlatform : IDomain
    {
        Definitions Definitions { get; }
        public Task<TResult> QueryAsync<TResult>(IQuery<TResult> query);
    }
}