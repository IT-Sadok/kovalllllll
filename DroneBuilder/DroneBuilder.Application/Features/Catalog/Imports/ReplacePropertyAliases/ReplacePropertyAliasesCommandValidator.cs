using FluentValidation;

namespace DroneBuilder.Application.Features.Catalog.Imports.ReplacePropertyAliases;

public sealed class ReplacePropertyAliasesCommandValidator : AbstractValidator<ReplacePropertyAliasesCommand>
{
    public ReplacePropertyAliasesCommandValidator()
    {
        RuleFor(command => command.SourceId).NotEmpty();
        RuleFor(command => command.PropertyId).NotEmpty();
        RuleFor(command => command.Model).NotNull();
        RuleForEach(command => command.Model.Aliases).NotEmpty().MaximumLength(200)
            .When(command => command.Model is not null);
    }
}
