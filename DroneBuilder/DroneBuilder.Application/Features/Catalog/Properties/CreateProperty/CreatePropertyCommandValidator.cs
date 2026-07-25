using DroneBuilder.Domain.Entities;
using FluentValidation;

namespace DroneBuilder.Application.Features.Catalog.Properties.CreateProperty;

public class CreatePropertyCommandValidator : AbstractValidator<CreatePropertyCommand>
{
    public CreatePropertyCommandValidator()
    {
        RuleFor(command => command.Model).NotNull();
        When(command => command.Model is not null, () =>
        {
            RuleFor(command => command.Model.Name).NotEmpty().MaximumLength(100);
            RuleFor(command => command.Model.Code).MaximumLength(100);
            RuleFor(command => command.Model.DataType)
                .NotEmpty()
                .Must(value => Enum.TryParse<SpecificationDataType>(value, true, out _))
                .WithMessage("Unsupported property data type.");
            RuleForEach(command => command.Model.Aliases).NotEmpty().MaximumLength(200);
            RuleFor(command => command.Model.Aliases)
                .Must(aliases => aliases.Distinct(StringComparer.OrdinalIgnoreCase).Count() == aliases.Count)
                .WithMessage("Property aliases must be unique.");
        });
    }
}
