using System.Threading.Tasks;
using Platformex;
using Platformex.Domain;

namespace Demo.Application.Sagas
{
    [EventSubscriber]
    public class ProcessDocumentSaga : Saga<ProcessDocumentSaga>,
        ISubscribeTo<DocumentId, DocumentCreated>
    
    {
        public async Task HandleAsync(IDomainEvent<DocumentId, DocumentCreated> domainEvent)
        {
            await Domain.CreateCar(CarId.New, domainEvent.AggregateEvent.Name);
        }
    }
}