using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Options;

namespace DroneBuilder.Application.Validation.Options;

public class FluentValidationOptions<TOptions>(IValidator<TOptions> validator) : IValidateOptions<TOptions>
    where TOptions : class
{
    public ValidateOptionsResult Validate(string? name, TOptions options)
    {
        if (validator == null)
        {
            return ValidateOptionsResult.Fail($"No validator provided for {typeof(TOptions).Name}");
        }

        ValidationResult result = validator.Validate(options);
        if (result.IsValid)
        {
            return ValidateOptionsResult.Success;
        }

        IEnumerable<string> errors = result.Errors.Select(e => $"Options validation failed for '{e.PropertyName}': {e.ErrorMessage}");
        return ValidateOptionsResult.Fail(errors);
    }
}
