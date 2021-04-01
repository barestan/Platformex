using System;
using System.Net;
using System.Threading.Tasks;
using Demo;
using Demo.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Platformex.Infrastructure;

namespace Platformex.Demo
{
    class Program
    {
        static async Task Main()
        {
            var platform = await CreateHost();

            var projectId = ProjectId.New;
            var project = await platform.CreateProject(projectId, "test");
            
            await platform.CreateProject(ProjectId.New, "test");

            await project.RenameProject("new-name");

            Console.ReadKey();
        }

        private static async Task<IClusterClient> CreateHost()
        {
            var builder = new SiloHostBuilder()
                .UseLocalhostClustering()
                .Configure<ClusterOptions>(options =>  { options.ClusterId = "dev"; options.ServiceId = "HelloWorldApp"; })
                .Configure<EndpointOptions>(options => options.AdvertisedIPAddress = IPAddress.Loopback)
                
                .AddSimpleMessageStreamProvider("EventBusProvider")
                .AddMemoryGrainStorage("PubSubStore")
                
                .ConfigureLogging(logging => logging.AddConsole())
                
                .ConfigurePlatformex(p =>
                {
                    p.WithContext<DemoContext>();
                });

            var host = builder.Build();
            await host.StartAsync();

            var platform = host.Services.GetService<IClusterClient>();
            return platform;
        }
    }
}
