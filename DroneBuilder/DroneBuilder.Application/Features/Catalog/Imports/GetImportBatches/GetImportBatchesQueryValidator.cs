using FluentValidation;

namespace DroneBuilder.Application.Features.Catalog.Imports.GetImportBatches;

public sealed class GetImportBatchesQueryValidator : AbstractValidator<GetImportBatchesQuery>
{
    public GetImportBatchesQueryValidator()
    {
        RuleFor(query => query.SourceId).NotEmpty();
    }
}
