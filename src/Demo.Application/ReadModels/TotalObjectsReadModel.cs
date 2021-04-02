using System.Threading.Tasks;
using Platformex;
using Platformex.Application;
using Platformex.Domain;

namespace Demo.Application.ReadModels
{
    //ReadModel - проекция, которая готовит данные для запроса
    [EventSubscriber]
    class TotalObjectsReadModel : ReadModel<TotalObjectsReadModel>,
        IAmReadModelFor<CarId, CarCreated>,
        IAmReadModelFor<DocumentId, DocumentCreated>
    {
        public static int Count { get; private set; }
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
    //Запрос
    public class TotalObjectsQuery : IQuery<TotalObjectsQueryResult>
    {

    }
    //Результат запроса
    public class TotalObjectsQueryResult
    {
        public TotalObjectsQueryResult(int count)
        {
            Count = count;
        }

        public int Count { get; }
    }

    
    //Обработчик запроса
    public class TotalObjectsQueryHandler : QueryHandler<TotalObjectsQuery, TotalObjectsQueryResult>
    {
        protected override Task<TotalObjectsQueryResult> ExecuteAsync(TotalObjectsQuery query)
        {
            return Task.FromResult(new TotalObjectsQueryResult(TotalObjectsReadModel.Count));
        }
    }
}