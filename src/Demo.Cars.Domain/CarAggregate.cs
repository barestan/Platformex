using System.Threading.Tasks;
using Platformex;
using Platformex.Domain;

namespace Demo.Cars.Domain
{
    public interface ICarState : IAggregateState<CarId>
    {
        public string Name { get; }
    }

    public class CarAggregate : Aggregate<CarId, ICarState>, ICar
    {
        public async Task<CommandResult> Do(CreateCar command)
        {
            await Emit(new CarCreated(command.Id, command.Name));
            return CommandResult.Success;
        }

        public async Task<CommandResult> Do(RenameCar command)
        {
            if (command.NewName == null)
                return new CommandResult(false, "Имя не может быть пустым!");

            await Emit(new CarRenamed(State.Id, command.NewName, State.Name));
            return CommandResult.Success;
        }
    }
}
