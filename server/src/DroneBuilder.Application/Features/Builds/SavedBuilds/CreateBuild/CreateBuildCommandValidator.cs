using FluentValidation;

namespace DroneBuilder.Application.Features.Builds.SavedBuilds.CreateBuild;

public class CreateBuildCommandValidator : AbstractValidator<CreateBuildCommand>
{
    public CreateBuildCommandValidator()
    {
        RuleFor(x => x.Model)
            .NotNull().WithMessage("Build data is required.")
            .SetValidator(new SaveBuildModelValidator());
    }
}
