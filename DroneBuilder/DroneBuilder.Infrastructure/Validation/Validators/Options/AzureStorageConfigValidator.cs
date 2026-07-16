using DroneBuilder.Infrastructure.Options;
using FluentValidation;

namespace DroneBuilder.Infrastructure.Validation.Validators.Options;

public class AzureStorageConfigValidator : AbstractValidator<AzureStorageConfig>
{
    public AzureStorageConfigValidator()
    {
        RuleFor(x => x.ConnectionString).NotEmpty().WithMessage("Azure Storage ConnectionString is required.");
        RuleFor(x => x.ContainerName).NotEmpty().WithMessage("Azure Storage ContainerName is required.");
    }
}
