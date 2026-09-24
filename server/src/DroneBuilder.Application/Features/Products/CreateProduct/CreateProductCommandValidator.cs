using FluentValidation;

namespace DroneBuilder.Application.Features.Products.CreateProduct;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Model)
            .NotNull().WithMessage("Product data is required.");

        When(x => x.Model != null, () =>
        {
            RuleFor(x => x.Model.Name)
                .NotEmpty().WithMessage("Product name is required.")
                .MaximumLength(200).WithMessage("Product name must not exceed 200 characters.");

            RuleFor(x => x.Model.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0.");

            RuleFor(x => x.Model.Category)
                .NotNull().WithMessage("Category is required.")
                .IsInEnum().WithMessage("Unknown category.");

            RuleFor(x => x.Model.Manufacturer)
                .MaximumLength(100).WithMessage("Manufacturer must not exceed 100 characters.");

            RuleFor(x => x.Model.WeightGrams)
                .InclusiveBetween(0.1m, 100000m).WithMessage("Weight must be between 0.1 and 100000 grams.")
                .When(x => x.Model.WeightGrams.HasValue);
        });
    }
}
