using FluentValidation;

namespace DroneBuilder.Application.Features.Catalog.Metadata.UpdateComponentType;

public sealed class UpdateComponentTypeCommandValidator : AbstractValidator<UpdateComponentTypeCommand>
{
    public UpdateComponentTypeCommandValidator()
    {
        RuleFor(command => command.ComponentTypeId).NotEmpty();
        RuleFor(command => command.Model).NotNull();
        RuleFor(command => command.Model)
            .Must(model => model.Code is not null || model.Name is not null || model.IsActive.HasValue)
            .WithMessage("At least one component type field must be provided.")
            .When(command => command.Model is not null);
        RuleFor(command => command.Model.Code).NotEmpty().MaximumLength(100)
            .When(command => command.Model?.Code is not null);
        RuleFor(command => command.Model.Name).NotEmpty().MaximumLength(200)
            .When(command => command.Model?.Name is not null);
    }
}
