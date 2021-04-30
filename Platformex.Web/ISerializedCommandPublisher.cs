using System.Threading;
using System.Threading.Tasks;

namespace Platformex.Web
{
    public interface ISerializedCommandPublisher
    {
        Task<CommandResult> PublishSerilizedCommandAsync(
            System.String name,
            System.Int32 version,
            System.String json,
            CancellationToken cancellationToken);
    }
}