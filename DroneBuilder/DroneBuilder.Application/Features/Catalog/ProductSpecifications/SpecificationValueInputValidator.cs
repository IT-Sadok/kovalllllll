using DroneBuilder.Application.Features.Catalog.ProductSpecifications.Models;
using FluentValidation;

namespace DroneBuilder.Application.Features.Catalog.ProductSpecifications;

public class SpecificationValueInputValidator<TModel> : AbstractValidator<TModel>
    where TModel : SpecificationValueInputModel
{
    public SpecificationValueInputValidator()
    {
        RuleFor(model => model)
            .Must(HasExactlyOneRepresentation)
            .WithMessage("Exactly one value representation must be provided.");

        RuleFor(model => model.TextValue)
            .MaximumLength(1000)
            .When(model => model.TextValue is not null);

        RuleFor(model => model.MinNumericValue)
            .NotNull()
            .When(model => model.MaxNumericValue.HasValue)
            .WithMessage("Both minimum and maximum values are required for a numeric range.");

        RuleFor(model => model.MaxNumericValue)
            .NotNull()
            .When(model => model.MinNumericValue.HasValue)
            .WithMessage("Both minimum and maximum values are required for a numeric range.");

        RuleFor(model => model)
            .Must(model => !model.MinNumericValue.HasValue ||
                           !model.MaxNumericValue.HasValue ||
                           model.MinNumericValue <= model.MaxNumericValue)
            .WithMessage("Minimum value cannot be greater than maximum value.");
    }

    private static bool HasExactlyOneRepresentation(TModel model)
    {
        int representations =
            Convert.ToInt32(model.ValueId.HasValue) +
            Convert.ToInt32(model.TextValue is not null) +
            Convert.ToInt32(model.NumericValue.HasValue) +
            Convert.ToInt32(model.MinNumericValue.HasValue || model.MaxNumericValue.HasValue) +
            Convert.ToInt32(model.BooleanValue.HasValue);

        return representations == 1;
    }
}
