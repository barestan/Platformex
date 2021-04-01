using System.Threading.Tasks;

namespace Platformex
{
    public interface ICanDo<in TCommand, TIdentity> where TCommand : ICommand<TIdentity> where TIdentity : Identity<TIdentity>
    {
        Task<CommandResult> Do(TCommand command);
    }
}