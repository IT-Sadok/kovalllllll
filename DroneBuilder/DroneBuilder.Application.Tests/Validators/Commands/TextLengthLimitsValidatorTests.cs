using DroneBuilder.Application.Mediator.Commands.PropertyCommands;
using DroneBuilder.Application.Mediator.Commands.ValueCommands;
using DroneBuilder.Application.Models.ProductModels;
using DroneBuilder.Application.Validation.Validators;
using FluentValidation.TestHelper;

namespace DroneBuilder.Application.Tests.Validators.Commands;

public class TextLengthLimitsValidatorTests
{
    private const int ColumnLength = 100;

    private static string TooLong => new('a', ColumnLength + 1);
    private static string AtLimit => new('a', ColumnLength);

    [Fact]
    public void CreateProperty_Should_Reject_Name_Longer_Than_Column()
    {
        var command = new CreatePropertyCommand(new CreatePropertyModel { Name = TooLong });

        TestValidationResult<CreatePropertyCommand> result = new CreatePropertyCommandValidator().TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Model.Name);
    }

    [Fact]
    public void CreateProperty_Should_Accept_Name_At_Column_Length()
    {
        var command = new CreatePropertyCommand(new CreatePropertyModel { Name = AtLimit });

        TestValidationResult<CreatePropertyCommand> result = new CreatePropertyCommandValidator().TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateProperty_Should_Reject_Name_Longer_Than_Column()
    {
        var command = new UpdatePropertyCommand(Guid.NewGuid(), new UpdatePropertyModel { Name = TooLong });

        TestValidationResult<UpdatePropertyCommand> result = new UpdatePropertyCommandValidator().TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Model.Name);
    }

    [Fact]
    public void CreateValue_Should_Reject_Text_Longer_Than_Column()
    {
        var command = new CreateValueCommand(new CreateValueModel { Text = TooLong, PropertyId = Guid.NewGuid() });

        TestValidationResult<CreateValueCommand> result = new CreateValueCommandValidator().TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Model.Text);
    }

    [Fact]
    public void CreateValue_Should_Accept_Text_At_Column_Length()
    {
        var command = new CreateValueCommand(new CreateValueModel { Text = AtLimit, PropertyId = Guid.NewGuid() });

        TestValidationResult<CreateValueCommand> result = new CreateValueCommandValidator().TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateValue_Should_Reject_Text_Longer_Than_Column()
    {
        var command = new UpdateValueCommand(Guid.NewGuid(), new UpdateValueModel { Text = TooLong });

        TestValidationResult<UpdateValueCommand> result = new UpdateValueCommandValidator().TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Model.Text);
    }
}
