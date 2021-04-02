using System;
using System.ComponentModel;
using System.Threading.Tasks;
using FluentValidation;
using Orleans;
using Platformex;

#region hack
namespace System.Runtime.CompilerServices
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class IsExternalInit{}
}
#endregion

namespace Demo
{
    //Идентификатор
    public class CarId : Identity<CarId>
    {
        public CarId(string value) : base(value) {}
    }
    
//Команды
    [Rules(typeof(CreateCarValidator))]
    public record CreateCar(CarId Id, string Name) : ICommand<CarId>;

    public record RenameCar(CarId Id, string NewName) : ICommand<CarId>;

//Бизнес-правила
    public class CreateCarValidator : Rules<CreateCar>
    {
        public CreateCarValidator() {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Name).NotEmpty().WithMessage("Имя не может быть пустым");
        }
    }

//События
    public record CarCreated(CarId Id, string Name) : IAggregateEvent<CarId>;
    public record CarRenamed(CarId Id, string NewName, string OldName) : IAggregateEvent<CarId>;

//Интерфейс агрегата
    public interface ICar : IAggregate<CarId>,
        ICanDo<CreateCar, CarId>,
        ICanDo<RenameCar, CarId>
    {
        public Task<CommandResult> RenameCar(string newName) 
            => Do(new RenameCar(this.GetId<CarId>() , newName));
    }
//Расширение для агрегата
    public static class CarExtension
    {
        public static ICar GetCar(this IGrainFactory platform, CarId id) 
            => platform.GetGrain<ICar>(id.ToString());
        public static async Task<ICar> CreateCar(this IGrainFactory platform, CarId id, string newName)
        {
            var car = platform.GetCar(id);
            var result = await car.Do(new CreateCar(id, newName));
            if (!result.IsSuccess) throw new Exception(result.Error);
            return car;
        }
    }
}