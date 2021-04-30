using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Demo.Application;
using Demo.Application.Queries;
using Demo.Cars.Domain;
using Demo.Documents.Domain;
using Demo.Infrastructure;
using Demo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans.Configuration;
using Orleans.Hosting;
using Platformex;
using Platformex.Infrastructure;

namespace Demo.Host
{
    class Program
    {
        static async Task Main()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory)?.FullName)
                .AddJsonFile("appsettings.json")
                .Build();

            var platform = await CreateHost(configuration);

            var carId = CarId.New;
            var car = await platform.CreateCar(carId, "test");
            
            await platform.CreateCar(CarId.New, "test");

            await car.RenameCar("new-name");

            var docId = DocumentId.New;
            var doc = await platform.CreateDocument(docId, "doc");
            
            await doc.RenameDocument("doc-new-name");

            var result = await platform.QueryAsync(new TotalObjectsQuery());
            Console.WriteLine($">> Total count: {result.Count}" );

            var res = await platform.QueryAsync(new ObjectsNamesQuery());
            Console.WriteLine($">> Names: {string.Join(",", res.Names)}" );

            var items = await platform.QueryAsync(new DocumentInfoQuery(10));
            foreach (var c in items)
            {
                Console.WriteLine($">> DOCUMENT INFO: ID:{c.Id} Name:{c.Name} Changes:{c.ChangesCount}" );
            }


            Console.ReadKey();
        }

        private static async Task<IPlatform> CreateHost(IConfiguration configuration)
        {
            var builder = new SiloHostBuilder()
                //Конфигурация кластера
                .UseLocalhostClustering()
                .Configure<ClusterOptions>(options =>  { options.ClusterId = "dev"; options.ServiceId = "HelloWorldApp"; })
                .Configure<EndpointOptions>(options => options.AdvertisedIPAddress = IPAddress.Loopback)
                
                //Конфигурация шины событий
                .AddSimpleMessageStreamProvider("EventBusProvider")
                .AddMemoryGrainStorage("PubSubStore")
                
                //Конфигурация журналирования
                .ConfigureLogging(logging =>
                {
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Information);
                })

                //Конфигурация приложения
                .ConfigurePlatformex(p =>
                {
                    p.RegisterAggregate<CarId, CarAggregate, CarState>();
                    p.RegisterAggregate<DocumentId, DocumentAggregate, DocumentState>();
                    p.RegisterApplicationParts<DocumentInfoReadModel>();
                })
                //Конфигурация сервисов
                .ConfigureServices(s =>
                {
                    s.AddDbContext<DemoContext>(options =>
                        options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

                    s.AddScoped<ICarStateProvider, CarStateProvider>();
                });

            //Запуск узла кластера
            var host = builder.Build();
            await host.StartAsync();

            return host.Services.GetService<IPlatform>();
        }
    }
}
