using System.ComponentModel.DataAnnotations;
using Demo.Application;

namespace Demo.Infrastructure.Data
{
    public class CarModel : ICarModel
    {
        [Key]
        [MaxLength(256)]
        public string Id { get; set; }

        [MaxLength(256)] 
        public string Name { get; set; }
    }

}
