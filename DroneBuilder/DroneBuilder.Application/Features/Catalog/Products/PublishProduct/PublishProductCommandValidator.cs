using FluentValidation;

namespace DroneBuilder.Application.Features.Catalog.Products.PublishProduct;

public sealed class PublishProductCommandValidator : AbstractValidator<PublishProductCommand>
{
    public PublishProductCommandValidator()
    {
        RuleFor(command => command.ProductId).NotEmpty();
    }
}
