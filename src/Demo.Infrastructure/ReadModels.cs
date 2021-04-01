using System.Threading.Tasks;
using Platformex;
using Platformex.Infrastructure;

namespace Demo.Infrastructure
{

    [EventSubscriber]
    class TotalProjectsReadModel : ReadModel<TotalProjectsReadModel>,
        IAmReadModelFor<ProjectId, ProjectCreated>
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
        public Task Apply(IDomainEvent<ProjectId, ProjectCreated> domainEvent)
        {
            Name = domainEvent.AggregateEvent.Name;
            Count++;
            return Task.CompletedTask;
        }

        public Task Apply(IDomainEvent<ProjectId, ProjectRenamed> domainEvent)
        {
            Name = domainEvent.AggregateEvent.NewName;
            Count++;
            return Task.CompletedTask;
        }
    }
}
