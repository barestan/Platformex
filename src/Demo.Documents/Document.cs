using System;
using System.ComponentModel;
using System.Threading.Tasks;
using FluentValidation;
using Orleans;
using Platformex;

#region hack
namespace System.Runtime.CompilerServices
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class IsExternalInit{}
}
#endregion

namespace Demo
{
//Идентификатор
    public class DocumentId : Identity<DocumentId>
    {
        public DocumentId(string value) : base(value) {}
    }
    
//Команды
    [Rules(typeof(CreateDocumentValidator))]
    public record CreateDocument(DocumentId Id, string Name) : ICommand<DocumentId>;

    public record RenameDocument(DocumentId Id, string NewName) : ICommand<DocumentId>;

//Бизнес-правила
    public class CreateDocumentValidator : Rules<CreateDocument>
    {
        public CreateDocumentValidator() {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Name).NotEmpty().WithMessage("Имя не может быть пустым");
        }
    }

//События
    public record DocumentCreated(DocumentId Id, string Name) : IAggregateEvent<DocumentId>;
    public record DocumentRenamed(DocumentId Id, string NewName, string OldName) : IAggregateEvent<DocumentId>;

//Интерфейс агрегата
    public interface IDocument : IAggregate<DocumentId>,
        ICanDo<CreateDocument, DocumentId>,
        ICanDo<RenameDocument, DocumentId>
    {
        public Task<CommandResult> RenameDocument(string newName) 
            => Do(new RenameDocument(this.GetId<DocumentId>() , newName));
    }
//Расширение для агрегата
    public static class DocumentExtension
    {
        public static IDocument GetDocument(this IGrainFactory platform, DocumentId id) 
            => platform.GetGrain<IDocument>(id.ToString());
        public static async Task<IDocument> CreateDocument(this IGrainFactory platform, DocumentId id, string newName)
        {
            var document = platform.GetDocument(id);
            var result = await document.Do(new CreateDocument(id, newName));
            if (!result.IsSuccess) throw new Exception(result.Error);
            return document;
        }
    }
}