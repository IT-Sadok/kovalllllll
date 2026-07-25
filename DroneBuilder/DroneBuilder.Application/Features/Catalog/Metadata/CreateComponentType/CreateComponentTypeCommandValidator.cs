using FluentValidation;

namespace DroneBuilder.Application.Features.Catalog.Metadata.CreateComponentType;

public sealed class CreateComponentTypeCommandValidator : AbstractValidator<CreateComponentTypeCommand>
{
    public CreateComponentTypeCommandValidator()
    {
        RuleFor(command => command.Model).NotNull();
        RuleFor(command => command.Model.Code).NotEmpty().MaximumLength(100)
            .When(command => command.Model is not null);
        RuleFor(command => command.Model.Name).NotEmpty().MaximumLength(200)
            .When(command => command.Model is not null);
    }
}
