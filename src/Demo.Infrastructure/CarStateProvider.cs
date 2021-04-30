using System.Threading.Tasks;
using Demo.Application;
using Demo.Infrastructure.Data;

namespace Demo.Infrastructure
{

    public class CarStateProvider : ICarStateProvider
    {
        private readonly DemoContext _context;

        public CarStateProvider(DemoContext context)
        {
            _context = context;
        }
        public async Task<ICarModel> FindAsync(CarId id) 
            => await _context.Cars.FindAsync(id.Value);

        public ICarModel Create(CarId id)
        {
            var model = new CarModel {Id = id.Value};
            _context.Add(model);
            return model;
        }

        public Task SaveChangesAsync() 
            => _context.SaveChangesAsync();
    }
}
