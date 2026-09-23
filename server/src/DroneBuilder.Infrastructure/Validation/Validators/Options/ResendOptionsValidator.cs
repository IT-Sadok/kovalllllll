using DroneBuilder.Infrastructure.Options;
using FluentValidation;

namespace DroneBuilder.Infrastructure.Validation.Validators.Options;

public class ResendOptionsValidator : AbstractValidator<ResendOptions>
{
    public ResendOptionsValidator()
    {
        RuleFor(x => x.ApiKey).NotEmpty().WithMessage("Resend ApiKey is required.");
        RuleFor(x => x.FromEmail).NotEmpty().WithMessage("Resend FromEmail is required.")
            .EmailAddress().WithMessage("Resend FromEmail must be a valid email address.");
        RuleFor(x => x.ConfirmationUrl).NotEmpty().WithMessage("Resend ConfirmationUrl is required.")
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("Resend ConfirmationUrl must be an absolute URL.");
    }
}
