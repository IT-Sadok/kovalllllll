using FluentValidation;

namespace DroneBuilder.Application.Features.Catalog.Values.CreateValue;

public class CreateValueCommandValidator : AbstractValidator<CreateValueCommand>
{
    public CreateValueCommandValidator()
    {
        RuleFor(command => command.Model).NotNull();
        When(command => command.Model is not null, () =>
        {
            RuleFor(command => command.Model.Text).NotEmpty().MaximumLength(100);
            RuleFor(command => command.Model.Code).MaximumLength(100);
            RuleFor(command => command.Model.PropertyId).NotEmpty();
            RuleFor(command => command.Model)
                .Must(model => !(model.NumericValue.HasValue && model.BooleanValue.HasValue))
                .WithMessage("NumericValue and BooleanValue cannot be used together.");
            RuleForEach(command => command.Model.Aliases).NotEmpty().MaximumLength(200);
        });
    }
}
