using DroneBuilder.Application.Mediator.Queries.OrderQueries;
using FluentValidation;

namespace DroneBuilder.Application.Validation.Validators;

public class GetAdminOrdersQueryValidator : AbstractValidator<GetAdminOrdersQuery>
{
    public GetAdminOrdersQueryValidator()
    {
        RuleFor(x => x.Pagination).NotNull().WithMessage("Pagination is required.");
        RuleFor(x => x.Pagination.Page).GreaterThan(0).When(x => x.Pagination != null).WithMessage("Page must be greater than 0.");
        RuleFor(x => x.Pagination.PageSize).GreaterThan(0).When(x => x.Pagination != null).WithMessage("PageSize must be greater than 0.");
    }
}

