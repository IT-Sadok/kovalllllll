using FluentValidation;

namespace DroneBuilder.Application.Features.Catalog.Products.MoveProductToDraft;

public sealed class MoveProductToDraftCommandValidator : AbstractValidator<MoveProductToDraftCommand>
{
    public MoveProductToDraftCommandValidator()
    {
        RuleFor(command => command.ProductId).NotEmpty();
    }
}
