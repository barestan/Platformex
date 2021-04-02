using Demo.Documents.Domain;
using Platformex.Application;

namespace Demo.Application
{
    public class DocumentState : AggregateState<DocumentId, DocumentState>, IDocumentState,
        ICanApply<DocumentCreated, DocumentId>,
        ICanApply<DocumentRenamed, DocumentId>
    {
        public string Name { get; private set; }

        public void Apply(DocumentCreated e) 
            => Name = e.Name;
        public void Apply(DocumentRenamed e)
            => Name = e.NewName;

    }
}