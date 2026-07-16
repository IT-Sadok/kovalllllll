using DroneBuilder.Application.Mediator.Commands.ImageCommands;
using FluentValidation;

namespace DroneBuilder.Application.Validation.Validators;

public class UploadImageCommandValidator : AbstractValidator<UploadImageCommand>
{
    public UploadImageCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty().WithMessage("ProductId is required.");
        RuleFor(x => x.File).NotNull().WithMessage("File is required.");
    }
}

