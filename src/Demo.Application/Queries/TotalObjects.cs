using System.Threading.Tasks;
using Platformex;
using Platformex.Application;
using Platformex.Domain;
// ReSharper disable ArrangeTypeModifiers
// ReSharper disable ClassNeverInstantiated.Global


namespace Demo.Application.Queries
{
    //ReadModel - проекция, которая готовит данные для запроса
    [EventSubscriber]
    class TotalObjectsReadModel : ReadModel<TotalObjectsReadModel>,
        ISubscribeTo<CarId, CarCreated>,
        ISubscribeTo<DocumentId, DocumentCreated>
    {
        public static int Count { get; private set; }
        protected override string GetReadModelId(IDomainEvent domainEvent) => "total";

        public Task HandleAsync(IDomainEvent<CarId, CarCreated> domainEvent)
        {
            Count++;
            return Task.CompletedTask;
        }

        public Task HandleAsync(IDomainEvent<DocumentId, DocumentCreated> domainEvent)
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