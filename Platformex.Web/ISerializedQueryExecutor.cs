using System.Threading;
using System.Threading.Tasks;

namespace Platformex.Web
{
    public interface ISerializedQueryExecutor
    {
        Task<System.Object> ExecuteQueryAsync(
            System.String name,
            System.String json,
            CancellationToken cancellationToken);
    }
}