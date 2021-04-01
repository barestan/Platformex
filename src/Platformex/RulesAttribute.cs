using System;
using FluentValidation;
using FluentValidation.Results;

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

    public interface IRules
    {
        ValidationResult Validate(object source);
    }

    public class Rules<T> : AbstractValidator<T>, IRules
    {
        public ValidationResult Validate(object source)
        {
            return base.Validate((T) source);
        }
    }
}