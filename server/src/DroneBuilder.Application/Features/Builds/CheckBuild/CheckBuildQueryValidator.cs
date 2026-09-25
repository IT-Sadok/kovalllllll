using FluentValidation;

namespace DroneBuilder.Application.Features.Builds.CheckBuild;

public class CheckBuildQueryValidator : AbstractValidator<CheckBuildQuery>
{
    public CheckBuildQueryValidator()
    {
        RuleFor(x => x.Items)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Add at least one product to the build.")
            .Must(items => items.Count <= 30).WithMessage("A build can have at most 30 different products.")
            .Must(items => items.Select(i => i.ProductId).Distinct().Count() == items.Count)
            .WithMessage("Each product can appear only once; use the quantity instead.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).NotEmpty().WithMessage("ProductId is required.");
            item.RuleFor(i => i.Quantity).InclusiveBetween(1, 20).WithMessage("Quantity must be between 1 and 20.");
        });
    }
}
