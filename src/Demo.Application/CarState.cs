using System.Threading.Tasks;
using Demo.Cars.Domain;
using Platformex.Application;

namespace Demo.Application
{
    public class CarState : AggregateState<CarId, CarState>, ICarState,
        ICanApply<CarCreated, CarId>,
        ICanApply<CarRenamed, CarId>
    {
        public string Name { get; private set; }

        public void Apply(CarCreated e) 
            => Name = e.Name;
        public void Apply(CarRenamed e)
            => Name = e.NewName;

        public override Task LoadState(CarId id)
        {
            Id = id;
            return Task.CompletedTask;
        }
    }
}