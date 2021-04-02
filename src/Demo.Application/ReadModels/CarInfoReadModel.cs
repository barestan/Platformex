using System.Collections.Generic;
using System.Linq;
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
        public static List<CarInfo> Items = new List<CarInfo>();
        public async Task Apply(IDomainEvent<CarId, CarCreated> domainEvent)
        {
            var item = GetOrCreateIem(domainEvent.AggregateIdentity);
            item.Name = domainEvent.AggregateEvent.Name;
            item.ChangesCount++;

            await Task.Delay(2000);
        }
        
        public async Task Apply(IDomainEvent<CarId, CarRenamed> domainEvent)
        {
            var item = GetOrCreateIem(domainEvent.AggregateIdentity);
            item.Name = domainEvent.AggregateEvent.NewName;
            item.ChangesCount++;

            await Task.Delay(2000);
        }

        private CarInfo GetOrCreateIem(CarId id)
        {
            var result = Items.FirstOrDefault(i => i.Id == id.Value);
            if (result == null)
            {
                result = new CarInfo
                {
                    Id = id.Value,
                };
                Items.Add(result);
            }
            return result;
        }
    }
    //Запрос
    public class CarInfoQuery : IQuery<IEnumerable<CarInfo>>
    {
        public CarInfoQuery(int take)
        {
            Take = take;
        }

        public int Take { get; }
    }
    //Результат запроса

    public class CarInfo
    {
        public string Id { get; internal set; }
        public string Name { get; internal set;}
        public int ChangesCount{ get; internal set;}
    }


    //Обработчик запроса
    public class CarInfoQueryHandler : QueryHandler<CarInfoQuery, IEnumerable<CarInfo>>
    {
        protected override Task<IEnumerable<CarInfo>> ExecuteAsync(CarInfoQuery query)
        {
            return Task.FromResult(CarInfoReadModel.Items.Take(query.Take));
        }
    }

}
