using System.Threading.Tasks;
using Demo.Domain;
using Platformex.Infrastructure;

namespace Demo.Infrastructure
{
    internal class ProjectState : AggregateState<ProjectId, ProjectState>, IProjectState,
        ICanApply<ProjectCreated, ProjectId>,
        ICanApply<ProjectRenamed, ProjectId>
    {
        public string Name { get; private set; }

        public void Apply(ProjectCreated e) 
            => Name = e.Name;
        public void Apply(ProjectRenamed e)
            => Name = e.NewName;

        public override Task LoadState(ProjectId id)
        {
            Id = id;
            return Task.CompletedTask;
        }
    }
}