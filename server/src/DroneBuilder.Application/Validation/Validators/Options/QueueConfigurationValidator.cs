using DroneBuilder.Application.Options;
using FluentValidation;

namespace DroneBuilder.Application.Validation.Validators.Options;

public class QueueConfigurationValidator : AbstractValidator<QueueConfiguration>
{
    public QueueConfigurationValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Queue name is required.");
        RuleFor(x => x.MaxRetryCount).GreaterThanOrEqualTo(0).WithMessage("MaxRetryCount must be at least 0.");
        RuleFor(x => x.PrefetchCount).GreaterThan(0).WithMessage("PrefetchCount must be greater than 0.");
    }
}
