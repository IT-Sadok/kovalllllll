using FluentValidation;

namespace DroneBuilder.Application.Features.Builds.CheckBuild;

public class CheckBuildQueryValidator : AbstractValidator<CheckBuildQuery>
{
    public CheckBuildQueryValidator()
    {
        this.RuleForBuildItems(x => x.Items);
    }
}
