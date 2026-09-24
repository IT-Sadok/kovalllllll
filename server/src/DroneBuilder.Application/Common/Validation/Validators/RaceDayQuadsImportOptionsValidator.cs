using DroneBuilder.Application.Common.Options;
using FluentValidation;

namespace DroneBuilder.Application.Common.Validation.Validators;

public class RaceDayQuadsImportOptionsValidator : AbstractValidator<RaceDayQuadsImportOptions>
{
    public RaceDayQuadsImportOptionsValidator()
    {
        RuleFor(x => x.BaseUrl)
            .Must(u => Uri.TryCreate(u, UriKind.Absolute, out _)).WithMessage("RaceDayQuads BaseUrl must be an absolute URL.");

        RuleFor(x => x.MaxProductsPerCategory)
            .InclusiveBetween(1, 250).WithMessage("RaceDayQuads MaxProductsPerCategory must be between 1 and 250.");

        RuleFor(x => x.MaxVariantsPerProduct)
            .InclusiveBetween(1, 50).WithMessage("RaceDayQuads MaxVariantsPerProduct must be between 1 and 50.");

        RuleFor(x => x.RequestDelayMs)
            .GreaterThanOrEqualTo(500).WithMessage("RaceDayQuads RequestDelayMs must be at least 500 to stay polite.");
    }
}
