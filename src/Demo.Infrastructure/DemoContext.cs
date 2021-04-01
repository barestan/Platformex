using Platformex.Infrastructure;

namespace Demo.Infrastructure
{
    public class DemoContext : Context
    {
        public DemoContext()
        {
            Register<ProjectId, Domain.Project, ProjectState>();
        }
    }
}