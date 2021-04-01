using System.Threading.Tasks;
using Platformex;
using Platformex.Domain;

namespace Demo.Domain
{
    public interface IProjectState : IAggregateState<ProjectId>
    {
        public string Name { get; }
    }

    public class Project : Aggregate<ProjectId, IProjectState>, IProject
    {
        public async Task<CommandResult> Do(CreateProject command)
        {
            await Emit(new ProjectCreated(command.Id, command.Name));
            return CommandResult.Success;
        }

        public async Task<CommandResult> Do(RenameProject command)
        {
            if (command.NewName == null)
                return new CommandResult(false, "Имя не может быть пустым!");

            await Emit(new ProjectRenamed(State.Id, command.NewName, State.Name));
            return CommandResult.Success;
        }
    }
}
