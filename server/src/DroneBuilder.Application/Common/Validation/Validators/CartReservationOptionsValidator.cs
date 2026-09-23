using DroneBuilder.Application.Common.Options;
using FluentValidation;

namespace DroneBuilder.Application.Common.Validation.Validators;

public class CartReservationOptionsValidator : AbstractValidator<CartReservationOptions>
{
    public CartReservationOptionsValidator()
    {
        RuleFor(x => x.TimeToLiveMinutes)
            .GreaterThan(0).WithMessage("Cart reservation TimeToLiveMinutes must be greater than 0.");

        RuleFor(x => x.SweepIntervalMinutes)
            .GreaterThan(0).WithMessage("Cart reservation SweepIntervalMinutes must be greater than 0.");

        RuleFor(x => x.SweepIntervalMinutes)
            .LessThanOrEqualTo(x => x.TimeToLiveMinutes)
            .WithMessage("Cart reservation SweepIntervalMinutes must not exceed TimeToLiveMinutes.");
    }
}
