using System.Threading.Tasks;
using Platformex;
using Platformex.Application;
using Platformex.Domain;

namespace Demo.Application.ReadModels
{
    [EventSubscriber]
    public class CarInfoReadModel : AggregateReadModel<CarId, CarInfoReadModel>,
        IAmReadModelFor<CarId, CarCreated>,
        IAmReadModelFor<CarId, CarRenamed>

    {
        public string Name { get; set; }
        public int Count { get; set; }
        public async Task Apply(IDomainEvent<CarId, CarCreated> domainEvent)
        {
            Name = domainEvent.AggregateEvent.Name;
            Count++;
            await Task.Delay(2000);
        }

        public async Task Apply(IDomainEvent<CarId, CarRenamed> domainEvent)
        {
            Name = domainEvent.AggregateEvent.NewName;
            Count++;
            await Task.Delay(2000);
        }
    }
}
