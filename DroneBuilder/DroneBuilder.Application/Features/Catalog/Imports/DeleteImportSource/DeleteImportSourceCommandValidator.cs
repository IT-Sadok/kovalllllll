using FluentValidation;

namespace DroneBuilder.Application.Features.Catalog.Imports.DeleteImportSource;

public sealed class DeleteImportSourceCommandValidator : AbstractValidator<DeleteImportSourceCommand>
{
    public DeleteImportSourceCommandValidator()
    {
        RuleFor(command => command.SourceId).NotEmpty();
    }
}
