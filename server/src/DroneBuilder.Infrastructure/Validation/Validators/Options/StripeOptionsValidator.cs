using DroneBuilder.Infrastructure.Options;
using FluentValidation;

namespace DroneBuilder.Infrastructure.Validation.Validators.Options;

public class StripeOptionsValidator : AbstractValidator<StripeOptions>
{
    public StripeOptionsValidator()
    {
        RuleFor(x => x.SecretKey).NotEmpty().WithMessage("Stripe SecretKey is required.")
            .Must(key => key.StartsWith("sk_", StringComparison.Ordinal) || key.StartsWith("rk_", StringComparison.Ordinal))
            .WithMessage("Stripe SecretKey must be a secret (sk_) or restricted (rk_) key, not a publishable one.");
        RuleFor(x => x.WebhookSecret).NotEmpty().WithMessage("Stripe WebhookSecret is required.")
            .Must(secret => secret.StartsWith("whsec_", StringComparison.Ordinal))
            .WithMessage("Stripe WebhookSecret must start with whsec_.");
        RuleFor(x => x.Currency).NotEmpty().Length(3).WithMessage("Stripe Currency must be a 3-letter ISO code.");
        RuleFor(x => x.SuccessUrl).Must(BeAbsoluteUrl).WithMessage("Stripe SuccessUrl must be an absolute URL.");
        RuleFor(x => x.CancelUrl).Must(BeAbsoluteUrl).WithMessage("Stripe CancelUrl must be an absolute URL.");
    }

    private static bool BeAbsoluteUrl(string url) => Uri.TryCreate(url, UriKind.Absolute, out _);
}
