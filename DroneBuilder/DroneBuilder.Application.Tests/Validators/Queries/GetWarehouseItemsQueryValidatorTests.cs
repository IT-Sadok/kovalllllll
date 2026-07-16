using DroneBuilder.Application.Mediator.Queries.WarehouseQueries;
using DroneBuilder.Application.Models;
using DroneBuilder.Application.Validation.Validators;
using FluentValidation.TestHelper;

namespace DroneBuilder.Application.Tests.Validators.Queries;

public class GetWarehouseItemsQueryValidatorTests
{
    private readonly GetWarehouseItemsQueryValidator _validator;

    public GetWarehouseItemsQueryValidatorTests()
    {
        _validator = new GetWarehouseItemsQueryValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Pagination_Is_Null()
    {
        var query = new GetWarehouseItemsQuery(null!);
        TestValidationResult<GetWarehouseItemsQuery> result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.Pagination);
    }

    [Fact]
    public void Should_Have_Error_When_Page_Is_Zero_Or_Negative()
    {
        var query = new GetWarehouseItemsQuery(new PaginationParams(0, 10));
        TestValidationResult<GetWarehouseItemsQuery> result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.Pagination.Page);
    }

    [Fact]
    public void Should_Have_Error_When_PageSize_Is_Zero_Or_Negative()
    {
        var query = new GetWarehouseItemsQuery(new PaginationParams(1, 0));
        TestValidationResult<GetWarehouseItemsQuery> result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.Pagination.PageSize);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Query_Is_Valid()
    {
        var query = new GetWarehouseItemsQuery(new PaginationParams(1, 20));
        TestValidationResult<GetWarehouseItemsQuery> result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
