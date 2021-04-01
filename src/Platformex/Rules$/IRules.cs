using FluentValidation.Results;

namespace Platformex
{
    public interface IRules
    {
        ValidationResult Validate(object source);
    }
}