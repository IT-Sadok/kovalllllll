using FluentValidation;

namespace DroneBuilder.Application.Features.Catalog.Products.UpdateProduct;

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required.");

        RuleFor(x => x.Model)
            .NotNull().WithMessage("Update payload is required.")
            .Must(model => model is not null &&
                           (model.Name is not null || model.Price.HasValue || model.Category is not null))
            .WithMessage("At least one product field must be provided.");

        When(x => x.Model != null, () =>
        {
            RuleFor(x => x.Model.Name)
                .MaximumLength(200).WithMessage("Product name must not exceed 200 characters.")
                .When(x => x.Model.Name != null);

            RuleFor(x => x.Model.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0.")
                .When(x => x.Model.Price.HasValue);

            RuleFor(x => x.Model.Category)
                .MaximumLength(100).WithMessage("Category must not exceed 100 characters.")
                .When(x => x.Model.Category != null);
        });
    }
}
