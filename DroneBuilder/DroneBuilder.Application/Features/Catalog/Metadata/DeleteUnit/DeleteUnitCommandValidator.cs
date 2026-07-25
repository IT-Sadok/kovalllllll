using FluentValidation;

namespace DroneBuilder.Application.Features.Catalog.Metadata.DeleteUnit;

public sealed class DeleteUnitCommandValidator : AbstractValidator<DeleteUnitCommand>
{
    public DeleteUnitCommandValidator()
    {
        RuleFor(command => command.UnitId).NotEmpty();
    }
}
