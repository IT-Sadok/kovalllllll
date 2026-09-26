using System.Linq.Expressions;
using DroneBuilder.Application.Features.Builds.CheckBuild;
using FluentValidation;

namespace DroneBuilder.Application.Features.Builds;

public static class BuildItemRules
{
    public static void RuleForBuildItems<T>(this AbstractValidator<T> validator,
        Expression<Func<T, IEnumerable<BuildItemModel>>> items)
    {
        validator.RuleFor(items)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Add at least one product to the build.")
            .Must(list => list.Count() <= 30).WithMessage("A build can have at most 30 different products.")
            .Must(list => list.Select(i => i.ProductId).Distinct().Count() == list.Count())
            .WithMessage("Each product can appear only once; use the quantity instead.");

        validator.RuleForEach(items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).NotEmpty().WithMessage("ProductId is required.");
            item.RuleFor(i => i.Quantity).InclusiveBetween(1, 20).WithMessage("Quantity must be between 1 and 20.");
        });
    }
}
