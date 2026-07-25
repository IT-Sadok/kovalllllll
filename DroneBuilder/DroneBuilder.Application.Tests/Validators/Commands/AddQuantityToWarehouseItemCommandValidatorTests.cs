using DroneBuilder.Application.Features.Inventory.AddQuantityToWarehouseItem;
using DroneBuilder.Application.Models.WarehouseModels;
using FluentValidation.TestHelper;

namespace DroneBuilder.Application.Tests.Validators.Commands;

public class AddQuantityToWarehouseItemCommandValidatorTests
{
    private readonly AddQuantityToWarehouseItemCommandValidator _validator;

    public AddQuantityToWarehouseItemCommandValidatorTests()
    {
        _validator = new AddQuantityToWarehouseItemCommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_WarehouseItemId_Is_Empty()
    {
        // Arrange
        var command = new AddQuantityToWarehouseItemCommand(Guid.Empty, new AddQuantityModel { QuantityToAdd = 5 });

        // Act
        TestValidationResult<AddQuantityToWarehouseItemCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.WarehouseItemId);
    }

    [Fact]
    public void Should_Have_Error_When_Quantity_Is_Zero_Or_Less()
    {
        var command = new AddQuantityToWarehouseItemCommand(Guid.NewGuid(), new AddQuantityModel { QuantityToAdd = 0 });
        TestValidationResult<AddQuantityToWarehouseItemCommand> result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Model.QuantityToAdd);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        var command = new AddQuantityToWarehouseItemCommand(Guid.NewGuid(), new AddQuantityModel { QuantityToAdd = 10 });
        TestValidationResult<AddQuantityToWarehouseItemCommand> result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
