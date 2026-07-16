using DroneBuilder.Application.Mediator.Commands.OrderCommands;
using FluentValidation;

namespace DroneBuilder.Application.Validation.Validators;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.ShippingDetails)
            .NotNull().WithMessage("Shipping details are required.");

        When(x => x.ShippingDetails != null, () =>
        {
            RuleFor(x => x.ShippingDetails.FullName)
                .NotEmpty().WithMessage("Full name is required.")
                .MaximumLength(200).WithMessage("Full name must not exceed 200 characters.");

            RuleFor(x => x.ShippingDetails.AddressLine1)
                .NotEmpty().WithMessage("Address line 1 is required.")
                .MaximumLength(300).WithMessage("Address line 1 must not exceed 300 characters.");

            RuleFor(x => x.ShippingDetails.AddressLine2)
                .MaximumLength(300).WithMessage("Address line 2 must not exceed 300 characters.");

            RuleFor(x => x.ShippingDetails.City)
                .NotEmpty().WithMessage("City is required.")
                .MaximumLength(100).WithMessage("City must not exceed 100 characters.");

            RuleFor(x => x.ShippingDetails.State)
                .MaximumLength(100).WithMessage("State must not exceed 100 characters.");

            RuleFor(x => x.ShippingDetails.PostalCode)
                .NotEmpty().WithMessage("Postal code is required.")
                .MaximumLength(20).WithMessage("Postal code must not exceed 20 characters.");

            RuleFor(x => x.ShippingDetails.Country)
                .NotEmpty().WithMessage("Country is required.")
                .MaximumLength(100).WithMessage("Country must not exceed 100 characters.");

            RuleFor(x => x.ShippingDetails.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required.")
                .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters.");
        });
    }
}
