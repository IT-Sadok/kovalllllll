using FluentValidation;

namespace DroneBuilder.Application.Features.Catalog.Values.UpdateValue;

public class UpdateValueCommandValidator : AbstractValidator<UpdateValueCommand>
{
    public UpdateValueCommandValidator()
    {
        RuleFor(command => command.ValueId).NotEmpty();
        RuleFor(command => command.Model).NotNull();
        When(command => command.Model is not null, () =>
        {
            RuleFor(command => command.Model)
                .Must(model => model.Code is not null || model.Text is not null || model.NumericValue.HasValue ||
                               model.ClearNumericValue || model.BooleanValue.HasValue ||
                               model.ClearBooleanValue || model.Aliases is not null)
                .WithMessage("At least one value field must be provided.");
            RuleFor(command => command.Model.Code).NotEmpty().MaximumLength(100)
                .When(command => command.Model.Code is not null);
            RuleFor(command => command.Model.Text).NotEmpty().MaximumLength(100)
                .When(command => command.Model.Text is not null);
            RuleFor(command => command.Model)
                .Must(model => !(model.NumericValue.HasValue && model.ClearNumericValue))
                .WithMessage("NumericValue and ClearNumericValue cannot be used together.");
            RuleFor(command => command.Model)
                .Must(model => !(model.BooleanValue.HasValue && model.ClearBooleanValue))
                .WithMessage("BooleanValue and ClearBooleanValue cannot be used together.");
            RuleForEach(command => command.Model.Aliases!).NotEmpty().MaximumLength(200)
                .When(command => command.Model.Aliases is not null);
        });
    }
}
