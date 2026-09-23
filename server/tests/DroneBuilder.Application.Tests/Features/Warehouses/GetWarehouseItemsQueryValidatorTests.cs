using DroneBuilder.Application.Common.Pagination;
using DroneBuilder.Application.Features.Warehouses.GetWarehouseItems;
using FluentValidation.TestHelper;
namespace DroneBuilder.Application.Tests.Features.Warehouses;

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
    public void Should_Have_Error_When_PageSize_Exceeds_The_Cap()
    {
        var query = new GetWarehouseItemsQuery(new PaginationParams(1, 101));
        TestValidationResult<GetWarehouseItemsQuery> result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.Pagination.PageSize);
    }

    [Fact]
    public void Should_Not_Have_Error_When_PageSize_Is_At_The_Cap()
    {
        var query = new GetWarehouseItemsQuery(new PaginationParams(1, 100));
        TestValidationResult<GetWarehouseItemsQuery> result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Not_Have_Error_When_Query_Is_Valid()
    {
        var query = new GetWarehouseItemsQuery(new PaginationParams(1, 20));
        TestValidationResult<GetWarehouseItemsQuery> result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
