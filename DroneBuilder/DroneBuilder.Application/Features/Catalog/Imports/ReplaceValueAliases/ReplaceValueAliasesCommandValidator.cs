using FluentValidation;

namespace DroneBuilder.Application.Features.Catalog.Imports.ReplaceValueAliases;

public sealed class ReplaceValueAliasesCommandValidator : AbstractValidator<ReplaceValueAliasesCommand>
{
    public ReplaceValueAliasesCommandValidator()
    {
        RuleFor(command => command.SourceId).NotEmpty();
        RuleFor(command => command.ValueId).NotEmpty();
        RuleFor(command => command.Model).NotNull();
        RuleForEach(command => command.Model.Aliases).NotEmpty().MaximumLength(200)
            .When(command => command.Model is not null);
    }
}
