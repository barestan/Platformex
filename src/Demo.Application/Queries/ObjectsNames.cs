using System.Collections.Generic;
using System.Threading.Tasks;
using Platformex;
using Platformex.Application;
using Platformex.Domain;
// ReSharper disable ArrangeTypeModifiers
// ReSharper disable ClassNeverInstantiated.Global

namespace Demo.Application.Queries
{
    //Проекция
    [EventSubscriber]
    class ObjectsNamesReadModel : ReadModel<ObjectsNamesReadModel>,
        ISubscribeTo<DocumentId, DocumentCreated>,
        ISubscribeTo<DocumentId, DocumentRenamed>
    {
        public static List<string> Names { get; private set; } = new List<string>();

        protected override string GetReadModelId(IDomainEvent domainEvent) => "names";

        private Task AddNewIfNotExits(string name)
        {
            if (!Names.Contains(name)) Names.Add(name);
            return Task.CompletedTask;
        }
        public Task HandleAsync(IDomainEvent<DocumentId, DocumentCreated> domainEvent) 
            => AddNewIfNotExits(domainEvent.AggregateEvent.Name);

        public Task HandleAsync(IDomainEvent<DocumentId, DocumentRenamed> domainEvent) 
            => AddNewIfNotExits(domainEvent.AggregateEvent.NewName);
    }
    //Запрос
    public class ObjectsNamesQuery : IQuery<ObjectsNamesQueryResult>
    {

    }
    //Результат запроса
    public class ObjectsNamesQueryResult
    {
        public ObjectsNamesQueryResult(List<string> result)
        {
            Names = result;
        }

        public List<string> Names { get; }
    }

    
    //Обработчик запроса
    public class ObjectsNamesQueryHandler : QueryHandler<ObjectsNamesQuery, ObjectsNamesQueryResult>
    {
        protected override Task<ObjectsNamesQueryResult> ExecuteAsync(ObjectsNamesQuery query)
        {
            return Task.FromResult(new ObjectsNamesQueryResult(ObjectsNamesReadModel.Names));
        }
    }
}