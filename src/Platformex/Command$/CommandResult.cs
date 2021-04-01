using FluentValidation.Results;

namespace Platformex
{
    public class CommandResult
    {
        public bool IsSuccess { get; }
        public string Error { get; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        private ValidationResult ValidationResult { get; }

        public CommandResult(bool isSuccess, string error)
        {
            IsSuccess = isSuccess;
            Error = error;
        }

        public CommandResult(ValidationResult result)
        {
            IsSuccess = result.IsValid;
            ValidationResult = result;
            Error = result.ToString();
        }

        public static CommandResult Success => new CommandResult(true, null);
        public static CommandResult Fail(string message) => new CommandResult(false, message);
    }

}