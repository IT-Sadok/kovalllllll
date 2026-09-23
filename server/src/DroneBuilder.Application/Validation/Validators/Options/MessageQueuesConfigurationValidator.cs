using DroneBuilder.Application.Options;
using FluentValidation;

namespace DroneBuilder.Application.Validation.Validators.Options;

public class MessageQueuesConfigurationValidator : AbstractValidator<MessageQueuesConfiguration>
{
    public MessageQueuesConfigurationValidator()
    {
        RuleFor(x => x.UserQueue).NotNull().SetValidator(new QueueConfigurationValidator());
        RuleFor(x => x.CartQueue).NotNull().SetValidator(new QueueConfigurationValidator());
        RuleFor(x => x.OrderQueue).NotNull().SetValidator(new QueueConfigurationValidator());
        RuleFor(x => x.ImageQueue).NotNull().SetValidator(new QueueConfigurationValidator());
        RuleFor(x => x.ProductQueue).NotNull().SetValidator(new QueueConfigurationValidator());
        RuleFor(x => x.WarehouseQueue).NotNull().SetValidator(new QueueConfigurationValidator());
    }
}
