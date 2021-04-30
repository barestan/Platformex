using System.Threading.Tasks;
using Demo.Cars.Domain;
using Platformex;
using Platformex.Application;

namespace Demo.Application
{
    public interface ICarModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
    public interface ICarStateProvider
    {
        Task<ICarModel> FindAsync(CarId id);
        ICarModel Create(CarId id);
        Task SaveChangesAsync();
    }

    public class CarState : AggregateState<CarId, CarState>, ICarState,
        ICanApply<CarCreated, CarId>,
        ICanApply<CarRenamed, CarId>
    {
        private readonly ICarStateProvider _provider;
        private ICarModel _model;

        public CarState(ICarStateProvider provider)
        {
            _provider = provider;
        }

        public string Name => _model.Name;

        public void Apply(CarCreated e) 
            => _model.Name = e.Name;
        public void Apply(CarRenamed e)
            => _model.Name = e.NewName;

        protected override async Task LoadStateInternal(CarId id)
        {
            _model = await _provider.FindAsync(id) ?? _provider.Create(id);
        }

        protected override async Task AfterApply(IAggregateEvent<CarId> id)
        {
            await _provider.SaveChangesAsync();
        }
    }
}