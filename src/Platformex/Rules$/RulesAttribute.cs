using System;

namespace Platformex
{
    public sealed class RulesAttribute : Attribute
    {
        public Type RulesType { get; }

        public RulesAttribute(Type rulesType)
        {
            RulesType = rulesType;
        }
    }
}