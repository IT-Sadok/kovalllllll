using FluentValidation;

namespace DroneBuilder.Application.Features.Catalog.Imports.UpdateImportSource;

public sealed class UpdateImportSourceCommandValidator : AbstractValidator<UpdateImportSourceCommand>
{
    public UpdateImportSourceCommandValidator()
    {
        RuleFor(command => command.SourceId).NotEmpty();
        RuleFor(command => command.Model).NotNull();
        RuleFor(command => command.Model)
            .Must(model => model.Code is not null || model.Name is not null || model.BaseUrl is not null ||
                           model.ClearBaseUrl || model.IsActive.HasValue)
            .WithMessage("At least one import source field must be provided.")
            .When(command => command.Model is not null);
        RuleFor(command => command.Model.Code).NotEmpty().MaximumLength(100)
            .When(command => command.Model?.Code is not null);
        RuleFor(command => command.Model.Name).NotEmpty().MaximumLength(200)
            .When(command => command.Model?.Name is not null);
        RuleFor(command => command.Model.BaseUrl).NotEmpty().MaximumLength(1000)
            .When(command => command.Model?.BaseUrl is not null);
        RuleFor(command => command.Model)
            .Must(model => !(model.BaseUrl is not null && model.ClearBaseUrl))
            .WithMessage("BaseUrl and ClearBaseUrl cannot be used together.")
            .When(command => command.Model is not null);
    }
}
