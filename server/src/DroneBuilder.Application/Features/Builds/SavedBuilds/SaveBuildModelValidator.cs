using FluentValidation;

namespace DroneBuilder.Application.Features.Builds.SavedBuilds;

public class SaveBuildModelValidator : AbstractValidator<SaveBuildModel>
{
    public SaveBuildModelValidator()
    {
        RuleFor(x => x.Name)
            .Must(name => !string.IsNullOrWhiteSpace(name)).WithMessage("Give the build a name.")
            .MaximumLength(100).WithMessage("The build name can be at most 100 characters.");

        this.RuleForBuildItems(x => x.Items);
    }
}
