using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#region hack
namespace System.Runtime.CompilerServices
{
    public class IsExternalInit{}
}
#endregion

namespace Platformex
{
    public record AggregateDefinition(Type IdentityType, Type AggreagteType, Type InterfaceType, Type StateType);

    public sealed class Definitions
    {
        private readonly Dictionary<Type, AggregateDefinition>
            _aggregateDefinitions = new Dictionary<Type, AggregateDefinition>();
        public AggregateDefinition Aggregate<TIdentity>() where TIdentity : Identity<TIdentity> 
            => _aggregateDefinitions[typeof(TIdentity)];

        public void Register(AggregateDefinition definition)
        {
            _aggregateDefinitions.Add(definition.IdentityType, definition);
        }

        public IEnumerable<Assembly> Assemblies =>
            _aggregateDefinitions.Values.SelectMany(i => new []
            {
                i.AggreagteType.Assembly,
                i.IdentityType.Assembly,
                i.InterfaceType.Assembly,
                i.StateType.Assembly
            }).Distinct();

    }
}