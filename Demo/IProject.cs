using System;
using System.Threading.Tasks;
using FluentValidation;
using Orleans;
using Platformex;

#region hack
namespace System.Runtime.CompilerServices
{
    public class IsExternalInit{}
}
#endregion

namespace Demo
{
    //Идентификатор
    public class ProjectId : Identity<ProjectId>
    {
        public ProjectId(string value) : base(value) {}
    }
    
    //Команды
    [Rules(typeof(CreateProjectValidator))]
    public record CreateProject(ProjectId Id, string Name) : ICommand<ProjectId>;

    public record RenameProject(ProjectId Id, string NewName) : ICommand<ProjectId>;

    //Бизнес-правила
    public class CreateProjectValidator : Rules<CreateProject>
    {
        public CreateProjectValidator() {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Name).Empty().WithMessage("Имя не может быть пустым");
        }
    }

    //События
    public record ProjectCreated(ProjectId Id, string Name) : IAggregateEvent<ProjectId>;
    public record ProjectRenamed(ProjectId Id, string NewName, string OldName) : IAggregateEvent<ProjectId>;

    //Интерфейс агрегата
    public interface IProject : IAggregate<ProjectId>,
        ICanDo<CreateProject, ProjectId>,
        ICanDo<RenameProject, ProjectId>
    {
        public Task<CommandResult> RenameProject(string newName) 
            => Do(new RenameProject(this.GetId<ProjectId>() , newName));
    }
    //Расширение для агрегата
    public static class ProjectExtension
    {
        public static IProject GetProject(this IGrainFactory platform, ProjectId id) 
            => platform.GetGrain<IProject>(id.ToString());
        public static async Task<IProject> CreateProject(this IGrainFactory platform, ProjectId id, string newName)
        {
            var project = platform.GetProject(id);
            var result = await project.Do(new CreateProject(id, newName));
            if (!result.IsSuccess) throw new Exception(result.Error);
            return project;
        }
    }


}