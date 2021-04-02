using System.Threading.Tasks;
using Platformex;
using Platformex.Application;
using Platformex.Domain;

namespace Demo.Application.ReadModels
{
    [EventSubscriber]
    class TotalObjectsReadModel : ReadModel<TotalObjectsReadModel>,
        IAmSyncReadModelFor<CarId, CarCreated>,
        IAmSyncReadModelFor<DocumentId, DocumentCreated>
    {
        public int Count { get; private set; }
        public Task Apply(IDomainEvent<CarId, CarCreated> domainEvent)
        {
            Count++;
            return Task.CompletedTask;
        }

        protected override string GetReadModelId(IDomainEvent domainEvent) => "total";
        public Task Apply(IDomainEvent<DocumentId, DocumentCreated> domainEvent)
        {
            Count++;
            return Task.CompletedTask;
        }
    }
}