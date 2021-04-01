using FluentValidation;
using FluentValidation.Results;

namespace Platformex
{
    public class Rules<T> : AbstractValidator<T>, IRules
    {
        public ValidationResult Validate(object source)
        {
            return base.Validate((T) source);
        }
    }
}