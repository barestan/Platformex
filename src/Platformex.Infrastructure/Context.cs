using System.Collections.Generic;
using System.Linq;

namespace Platformex.Infrastructure
{
    public class Context : IContext
    {
        private readonly List<AggregateDefinition> _definitions = new List<AggregateDefinition>();
        public IEnumerable<AggregateDefinition> AggregateDefinitions => _definitions;

        protected void Register<TIdentity, TAggragate, TState>()
            where TIdentity : Identity<TIdentity>
            where TAggragate : class, IAggregate<TIdentity>
            where TState : AggregateState<TIdentity, TState>
        {
            var aggregateInterfaceType = typeof(TAggragate).GetInterfaces()
                .First(i => i.GetInterfaces().Any(j=> j.IsGenericType && j.GetGenericTypeDefinition() == typeof(IAggregate<>)));
            var info = new AggregateDefinition(typeof(TIdentity), typeof(TAggragate),
                aggregateInterfaceType, typeof(TState));

            _definitions.Add(info);
        }
    }
}