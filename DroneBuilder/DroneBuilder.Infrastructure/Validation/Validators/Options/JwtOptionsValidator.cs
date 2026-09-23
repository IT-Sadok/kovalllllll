using DroneBuilder.Infrastructure.Options;
using FluentValidation;

namespace DroneBuilder.Infrastructure.Validation.Validators.Options;

public class JwtOptionsValidator : AbstractValidator<JwtOptions>
{
    public JwtOptionsValidator()
    {
        RuleFor(x => x.Issuer).NotEmpty().WithMessage("Jwt Issuer is required.");
        RuleFor(x => x.Audience).NotEmpty().WithMessage("Jwt Audience is required.");
        RuleFor(x => x.Key).NotEmpty().MinimumLength(32).WithMessage("Jwt Key must be at least 32 characters long.");
        RuleFor(x => x.ExpiryMinutes).GreaterThan(0).WithMessage("Jwt ExpiryMinutes must be greater than 0.");
    }
}
