using FluentValidation;

namespace DroneBuilder.Application.Features.Builds.SavedBuilds.UpdateBuild;

public class UpdateBuildCommandValidator : AbstractValidator<UpdateBuildCommand>
{
    public UpdateBuildCommandValidator()
    {
        RuleFor(x => x.BuildId)
            .NotEmpty().WithMessage("Build ID is required.");

        RuleFor(x => x.Model)
            .NotNull().WithMessage("Build data is required.")
            .SetValidator(new SaveBuildModelValidator());
    }
}
