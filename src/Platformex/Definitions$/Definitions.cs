using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

#region hack
namespace System.Runtime.CompilerServices
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class IsExternalInit{}
}
#endregion


namespace Platformex
{
    public record AggregateDefinition(Type IdentityType, Type AggreagteType, Type InterfaceType, Type StateType);

    public sealed class Definitions
    {
        private readonly Dictionary<Type, AggregateDefinition>
            _aggregateDefinitions = new();
        public AggregateDefinition Aggregate<TIdentity>() where TIdentity : Identity<TIdentity> 
            => _aggregateDefinitions[typeof(TIdentity)];
        private readonly List<Assembly> _applicationPartsAssemlies = new();

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
            }).Concat(_applicationPartsAssemlies)
              .Distinct();

        public void RegisterApplicationParts(Assembly contextAppliactionParts)
        {
            if (!_applicationPartsAssemlies.Contains(contextAppliactionParts))
                _applicationPartsAssemlies.Add(contextAppliactionParts);
        }
    }
}