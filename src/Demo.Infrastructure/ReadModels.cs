using System.Threading.Tasks;
using Platformex;
using Platformex.Domain;
using Platformex.Infrastructure;

namespace Demo.Infrastructure
{

    [EventSubscriber]
    class TotalProjectsReadModel : ReadModel<TotalProjectsReadModel>,
        IAmSyncReadModelFor<ProjectId, ProjectCreated>
    {
        public int Count { get; private set; }
        public Task Apply(IDomainEvent<ProjectId, ProjectCreated> domainEvent)
        {
            Count++;
            return Task.CompletedTask;
        }

        protected override string GetReadModelId(IDomainEvent domainEvent) => "total";
    }

    [EventSubscriber]
    public class ProjectInfoReadModel : AggregateReadModel<ProjectId, ProjectInfoReadModel>,
        IAmReadModelFor<ProjectId, ProjectCreated>,
        IAmReadModelFor<ProjectId, ProjectRenamed>

    {
        public string Name { get; set; }
        public int Count { get; set; }
        public async Task Apply(IDomainEvent<ProjectId, ProjectCreated> domainEvent)
        {
            Name = domainEvent.AggregateEvent.Name;
            Count++;
            await Task.Delay(2000);
        }

        public async Task Apply(IDomainEvent<ProjectId, ProjectRenamed> domainEvent)
        {
            Name = domainEvent.AggregateEvent.NewName;
            Count++;
            await Task.Delay(2000);
        }
    }
}
