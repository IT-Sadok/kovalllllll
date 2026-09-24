using FluentValidation;

namespace DroneBuilder.Application.Features.Imports.GetImportRunById;

public class GetImportRunByIdQueryValidator : AbstractValidator<GetImportRunByIdQuery>
{
    public GetImportRunByIdQueryValidator()
    {
        RuleFor(x => x.ImportRunId).NotEmpty().WithMessage("ImportRunId is required.");
    }
}
