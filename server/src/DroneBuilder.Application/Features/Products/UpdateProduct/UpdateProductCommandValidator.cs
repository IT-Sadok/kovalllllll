using FluentValidation;

namespace DroneBuilder.Application.Features.Products.UpdateProduct;

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required.");

        RuleFor(x => x.Model)
            .NotNull().WithMessage("Product data is required.");

        When(x => x.Model != null, () =>
        {
            RuleFor(x => x.Model.Name)
                .NotEmpty().WithMessage("Product name must not be empty.")
                .MaximumLength(200).WithMessage("Product name must not exceed 200 characters.")
                .When(x => x.Model.Name != null);

            RuleFor(x => x.Model.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0.")
                .When(x => x.Model.Price.HasValue);

            RuleFor(x => x.Model.Category)
                .IsInEnum().WithMessage("Unknown category.")
                .When(x => x.Model.Category.HasValue);

            RuleFor(x => x.Model.Manufacturer)
                .MaximumLength(100).WithMessage("Manufacturer must not exceed 100 characters.");

            RuleFor(x => x.Model.WeightGrams)
                .InclusiveBetween(0m, 100000m).WithMessage("Weight must be between 0 and 100000 grams.")
                .When(x => x.Model.WeightGrams.HasValue);
        });
    }
}
