using FluentValidation;

namespace DroneBuilder.Application.Features.Catalog.Products.GetAdminProductById;

public sealed class GetAdminProductByIdQueryValidator : AbstractValidator<GetAdminProductByIdQuery>
{
    public GetAdminProductByIdQueryValidator()
    {
        RuleFor(query => query.ProductId).NotEmpty();
    }
}
