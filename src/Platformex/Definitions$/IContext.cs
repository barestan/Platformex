using System.Collections.Generic;

namespace Platformex
{
    public interface IContext
    {
        IEnumerable<AggregateDefinition> AggregateDefinitions { get; }

    }
}