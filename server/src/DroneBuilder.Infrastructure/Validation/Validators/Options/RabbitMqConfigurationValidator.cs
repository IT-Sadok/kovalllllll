using DroneBuilder.Infrastructure.MessageBroker.Configuration;
using FluentValidation;

namespace DroneBuilder.Infrastructure.Validation.Validators.Options;

public class RabbitMqConfigurationValidator : AbstractValidator<RabbitMqConfiguration>
{
    public RabbitMqConfigurationValidator()
    {
        RuleFor(x => x.HostName).NotEmpty().WithMessage("RabbitMQ HostName is required.");
        RuleFor(x => x.Port).GreaterThan(0).WithMessage("RabbitMQ Port must be greater than 0.");
        RuleFor(x => x.UserName).NotEmpty().WithMessage("RabbitMQ UserName is required.");
        RuleFor(x => x.Password).NotEmpty().WithMessage("RabbitMQ Password is required.");
    }
}
