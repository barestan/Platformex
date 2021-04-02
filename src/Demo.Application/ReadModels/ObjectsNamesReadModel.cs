using System.Collections.Generic;
using System.Threading.Tasks;
using Platformex;
using Platformex.Application;
using Platformex.Domain;

namespace Demo.Application.ReadModels
{
    [EventSubscriber]
    class ObjectsNamesReadModel : ReadModel<ObjectsNamesReadModel>,
        IAmReadModelFor<CarId, CarCreated>,
        IAmReadModelFor<DocumentId, DocumentCreated>,
        IAmReadModelFor<CarId, CarRenamed>,
        IAmReadModelFor<DocumentId, DocumentRenamed>
    {
        public List<string> Names { get; private set; } = new List<string>();

        protected override string GetReadModelId(IDomainEvent domainEvent) => "names";

        private Task AddNewIfNotExits(string name)
        {
            if (!Names.Contains(name)) Names.Add(name);
            return Task.CompletedTask;
        }
        public Task Apply(IDomainEvent<CarId, CarCreated> domainEvent) 
            => AddNewIfNotExits(domainEvent.AggregateEvent.Name);

        public Task Apply(IDomainEvent<DocumentId, DocumentCreated> domainEvent) 
            => AddNewIfNotExits(domainEvent.AggregateEvent.Name);

        public Task Apply(IDomainEvent<CarId, CarRenamed> domainEvent) 
            => AddNewIfNotExits(domainEvent.AggregateEvent.NewName);

        public Task Apply(IDomainEvent<DocumentId, DocumentRenamed> domainEvent) 
            => AddNewIfNotExits(domainEvent.AggregateEvent.NewName);
    }
}