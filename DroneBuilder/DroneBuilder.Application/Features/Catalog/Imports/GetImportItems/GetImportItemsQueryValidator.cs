using FluentValidation;

namespace DroneBuilder.Application.Features.Catalog.Imports.GetImportItems;

public sealed class GetImportItemsQueryValidator : AbstractValidator<GetImportItemsQuery>
{
    public GetImportItemsQueryValidator()
    {
        RuleFor(query => query.SourceId).NotEmpty();
        RuleFor(query => query.BatchId).NotEmpty();
    }
}
