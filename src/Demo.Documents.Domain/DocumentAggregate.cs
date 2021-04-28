using System.Threading.Tasks;
using Platformex;
using Platformex.Domain;
// ReSharper disable ClassNeverInstantiated.Global

namespace Demo.Documents.Domain
{
    public interface IDocumentState : IAggregateState<DocumentId>
    {
        public string Name { get; }
    }

    public class DocumentAggregate : Aggregate<DocumentId, IDocumentState>, IDocument
    {
        public async Task<CommandResult> Do(CreateDocument command)
        {
            await Emit(new DocumentCreated(command.Id, command.Name));
            return CommandResult.Success;
        }

        public async Task<CommandResult> Do(RenameDocument command)
        {
            if (command.NewName == null)
                return new CommandResult(false, "Имя не может быть пустым!");

            await Emit(new DocumentRenamed(State.Id, command.NewName, State.Name));
            return CommandResult.Success;
        }
    }
}
