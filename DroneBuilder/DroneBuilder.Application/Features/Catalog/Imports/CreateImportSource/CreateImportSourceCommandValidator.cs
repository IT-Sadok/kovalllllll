using FluentValidation;

namespace DroneBuilder.Application.Features.Catalog.Imports.CreateImportSource;

public sealed class CreateImportSourceCommandValidator : AbstractValidator<CreateImportSourceCommand>
{
    public CreateImportSourceCommandValidator()
    {
        RuleFor(command => command.Model).NotNull();
        RuleFor(command => command.Model.Code).NotEmpty().MaximumLength(100)
            .When(command => command.Model is not null);
        RuleFor(command => command.Model.Name).NotEmpty().MaximumLength(200)
            .When(command => command.Model is not null);
        RuleFor(command => command.Model.BaseUrl).MaximumLength(1000)
            .When(command => command.Model?.BaseUrl is not null);
    }
}
