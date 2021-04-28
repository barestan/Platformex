using Demo.Documents.Domain;
using Platformex.Application;
// ReSharper disable ClassNeverInstantiated.Global

namespace Demo.Application
{
    public class DocumentState : AggregateState<DocumentId, DocumentState>, IDocumentState,
        ICanApply<DocumentCreated, DocumentId>,
        ICanApply<DocumentRenamed, DocumentId>
    {
        private readonly DocumentDataModel _dataModel = new DocumentDataModel();

        public string Name
        {
            get => _dataModel.Name;
            private set => _dataModel.Name = value;
        }

        public void Apply(DocumentCreated e) 
            => Name = e.Name;
        public void Apply(DocumentRenamed e)
            => Name = e.NewName;
        
    }

    public class DocumentDataModel    
    {
        public string Name { get; set; }
    }

}