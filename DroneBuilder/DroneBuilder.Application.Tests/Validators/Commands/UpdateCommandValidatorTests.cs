using DroneBuilder.Application.Features.Catalog.Products.UpdateProduct;
using DroneBuilder.Application.Features.Catalog.Properties.UpdateProperty;
using DroneBuilder.Application.Features.Catalog.Values.UpdateValue;
using DroneBuilder.Application.Models.ProductModels;
using FluentValidation.TestHelper;

namespace DroneBuilder.Application.Tests.Validators.Commands;

public class UpdateCommandValidatorTests
{
    [Fact]
    public void ProductUpdate_EmptyPayload_IsRejected()
    {
        var validator = new UpdateProductCommandValidator();
        var command = new UpdateProductCommand(Guid.NewGuid(), new UpdateProductRequestModel());

        TestValidationResult<UpdateProductCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Model);
    }

    [Fact]
    public void ProductUpdate_WithOneField_IsAccepted()
    {
        var validator = new UpdateProductCommandValidator();
        var command = new UpdateProductCommand(
            Guid.NewGuid(),
            new UpdateProductRequestModel { Name = "Updated product" });

        TestValidationResult<UpdateProductCommand> result = validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void PropertyUpdate_EmptyPayload_IsRejected()
    {
        var validator = new UpdatePropertyCommandValidator();
        var command = new UpdatePropertyCommand(Guid.NewGuid(), new UpdatePropertyModel());

        TestValidationResult<UpdatePropertyCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Model);
    }

    [Fact]
    public void ValueUpdate_EmptyPayload_IsRejected()
    {
        var validator = new UpdateValueCommandValidator();
        var command = new UpdateValueCommand(Guid.NewGuid(), new UpdateValueModel());

        TestValidationResult<UpdateValueCommand> result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Model);
    }
}
