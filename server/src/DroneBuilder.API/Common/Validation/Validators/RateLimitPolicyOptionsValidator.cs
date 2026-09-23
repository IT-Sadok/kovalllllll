using DroneBuilder.API.Common.Options;
using FluentValidation;

namespace DroneBuilder.API.Common.Validation.Validators;

public class RateLimitPolicyOptionsValidator : AbstractValidator<RateLimitPolicyOptions>
{
    public RateLimitPolicyOptionsValidator()
    {
        RuleFor(x => x.PermitLimit).GreaterThan(0).WithMessage("PermitLimit must be greater than 0.");
        RuleFor(x => x.WindowInMinutes).GreaterThan(0).WithMessage("WindowInMinutes must be greater than 0.");
    }
}
