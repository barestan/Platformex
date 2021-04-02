using System.Linq;
using Platformex.Application;

namespace Platformex.Infrastructure
{
    public class Platform : IPlatform
    {
        public Definitions Definitions { get; } = new Definitions();

        public void RegisterApplicationParts<T>()
        {
            Definitions.RegisterApplicationParts(typeof(T).Assembly);
        }

        public void RegisterAggregate<TIdentity, TAggragate, TState>()
            where TIdentity : Identity<TIdentity>
            where TAggragate : class, IAggregate<TIdentity>
            where TState : AggregateState<TIdentity, TState>
        {
            var aggregateInterfaceType = typeof(TAggragate).GetInterfaces()
                .First(i => i.GetInterfaces().Any(j=> j.IsGenericType && j.GetGenericTypeDefinition() == typeof(IAggregate<>)));
            var info = new AggregateDefinition(typeof(TIdentity), typeof(TAggragate),
                aggregateInterfaceType, typeof(TState));

            Definitions.Register(info);
        }
    }
}