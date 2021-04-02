using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.ApplicationParts;
using Orleans.Hosting;

namespace Platformex.Infrastructure
{
    public static class SiloHostBuilderExtension
    {
        public static ISiloHostBuilder ConfigurePlatformex(this ISiloHostBuilder builder, Action<Platform> configureAction)
        {
            var platform = new Platform();

            configureAction(platform);

            foreach (var asm in platform.Definitions.Assemblies)
            {
                builder.ConfigureApplicationParts(manager =>
                {
                    manager.AddApplicationPart(new AssemblyPart(asm));
                });
            }

            builder.ConfigureServices(collection =>
            {
                collection.AddSingleton<IPlatform>(platform);
            });

            builder
                .AddStartupTask((provider, _) => Initializer.InitAsync(provider))
                .ConfigureApplicationParts(manager =>
                    manager.AddApplicationPart(typeof(Initializer).Assembly).WithReferences());

            //ѕроверка бизнес-правил на стороне клиента
            builder.AddOutgoingGrainCallFilter(async context =>
            {
                if (context.Arguments?.Length == 1)
                {
                    var argument = context.Arguments[0];
                    var rulesAttribute = argument.GetType().GetCustomAttribute<RulesAttribute>();
                    if (rulesAttribute != null)
                    {
                        var rules = (IRules)Activator.CreateInstance(rulesAttribute.RulesType);
                        var result = rules.Validate(argument);
                        if (!result.IsValid)
                        {
                            context.Result = new CommandResult(result);
                            return;
                        }
                    }
                }

                await context.Invoke();

                
            });

            return builder;
        }
    }
}