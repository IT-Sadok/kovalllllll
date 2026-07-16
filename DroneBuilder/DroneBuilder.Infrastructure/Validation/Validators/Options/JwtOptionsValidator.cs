using DroneBuilder.Infrastructure.Options;
using FluentValidation;

namespace DroneBuilder.Infrastructure.Validation.Validators.Options;

public class JwtOptionsValidator : AbstractValidator<JwtOptions>
{
    public JwtOptionsValidator()
    {
        RuleFor(x => x.Issuer).NotEmpty().WithMessage("Jwt Issuer is required.");
        RuleFor(x => x.Audience).NotEmpty().WithMessage("Jwt Audience is required.");
        RuleFor(x => x.Key).NotEmpty().MinimumLength(16).WithMessage("Jwt Key must be at least 16 characters long.");
    }
}
