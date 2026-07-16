using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DroneBuilder.Application.Validation.Options;

public class FluentValidationOptions<TOptions>(IServiceProvider serviceProvider) : IValidateOptions<TOptions>
    where TOptions : class
{
    public ValidateOptionsResult Validate(string? name, TOptions options)
    {
        using var scope = serviceProvider.CreateScope();
        var validator = scope.ServiceProvider.GetService<IValidator<TOptions>>();

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
