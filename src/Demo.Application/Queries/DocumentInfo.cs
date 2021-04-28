using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Platformex;
using Platformex.Application;
using Platformex.Domain;

namespace Demo.Application.Queries
{
    [EventSubscriber]
    public class DocumentInfoReadModel : AggregateReadModel<DocumentId, DocumentInfoReadModel>,
        ISubscribeSyncTo<DocumentId, DocumentCreated>,
        ISubscribeSyncTo<DocumentId, DocumentRenamed>

    {
        public static List<DocumentInfo> Items = new List<DocumentInfo>();
        public Task HandleAsync(IDomainEvent<DocumentId, DocumentCreated> domainEvent)
        {
            var item = GetOrCreateItem(domainEvent.AggregateIdentity);
            item.Name = domainEvent.AggregateEvent.Name;
            item.ChangesCount++;
            return Task.CompletedTask;
        }
        
        public Task HandleAsync(IDomainEvent<DocumentId, DocumentRenamed> domainEvent)
        {
            var item = GetOrCreateItem(domainEvent.AggregateIdentity);
            item.Name = domainEvent.AggregateEvent.NewName;
            item.ChangesCount++;
            return Task.CompletedTask;
        }

        private DocumentInfo GetOrCreateItem(DocumentId id)
        {
            var result = Items.FirstOrDefault(i => i.Id == id.Value);
            if (result == null)
            {
                result = new DocumentInfo
                {
                    Id = id.Value,
                };
                Items.Add(result);
            }
            return result;
        }
    }
    //Запрос
    public class DocumentInfoQuery : IQuery<IEnumerable<DocumentInfo>>
    {
        public DocumentInfoQuery(int take)
        {
            Take = take;
        }

        public int Take { get; }
    }
    //Результат запроса

    public class DocumentInfo
    {
        public string Id { get; internal set; }
        public string Name { get; internal set;}
        public int ChangesCount{ get; internal set;}
    }


    //Обработчик запроса
    public class DocumentInfoQueryHandler : QueryHandler<DocumentInfoQuery, IEnumerable<DocumentInfo>>
    {
        protected override Task<IEnumerable<DocumentInfo>> ExecuteAsync(DocumentInfoQuery query)
        {
            return Task.FromResult(DocumentInfoReadModel.Items.Take(query.Take));
        }
    }

}
