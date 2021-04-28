using System;
using System.Threading.Tasks;
using Platformex;

namespace Demo
{
//Расширение для агрегата
    public static class CarService
    {
        public static ICar GetCar(this IDomain domain, CarId id) 
            => domain.GetAggregate<ICar>(id.Value);
        public static async Task<ICar> CreateCar(this IDomain domain, CarId id, string newName)
        {
            var car = domain.GetCar(id);
            var result = await car.Do(new CreateCar(id, newName));
            if (!result.IsSuccess) throw new Exception(result.Error);
            return car;
        }
    }
}
