using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Mediator.Commands.ProductCommands;

public class AddValueToProductPropertyCommandHandler(
    IProductRepository productRepository,
    IPropertyRepository propertyRepository,
    IValueRepository valueRepository)
    : ICommandHandler<AddValueToProductPropertyCommand>
{
    public async Task<Result> ExecuteCommandAsync(AddValueToProductPropertyCommand command, CancellationToken cancellationToken)
    {
        Product? product = await productRepository.GetProductByIdAsync(command.ProductId, cancellationToken);
        if (product == null)
        {
            return Result.Fail(new NotFoundError($"Product with ID {command.ProductId} not found."));
        }

        Property? property = await propertyRepository.GetPropertyByIdAsync(command.PropertyId, cancellationToken);
        if (property == null)
        {
            return Result.Fail(new NotFoundError($"Property with ID {command.PropertyId} not found."));
        }

        Value? value = await valueRepository.GetValueByIdAsync(command.ValueId, cancellationToken);
        if (value == null)
        {
            return Result.Fail(new NotFoundError($"Value with ID {command.ValueId} not found."));
        }

        if (product.ProductPropertyValues != null &&
            product.ProductPropertyValues.Any(p => p.PropertyId == command.PropertyId && p.ValueId == command.ValueId))
        {
            return Result.Fail(new ValidationError($"Value with ID {command.ValueId} is already associated with Property ID {command.PropertyId} on Product ID {command.ProductId}."));
        }

        if (property.DataType != SpecificationDataType.Option)
        {
            return Result.Fail(new ValidationError(
                $"Property with ID {property.Id} does not accept predefined option values."));
        }

        if (!property.Values.Any(item => item.Id == value.Id))
        {
            return Result.Fail(new ValidationError(
                $"Value with ID {value.Id} is not allowed for Property ID {property.Id}."));
        }

        try
        {
            product.AddSpecification(new ProductPropertyValue
            {
                PropertyId = property.Id,
                Property = property,
                ValueId = value.Id,
                Value = value
            });
        }
        catch (InvalidOperationException exception)
        {
            return Result.Fail(new ValidationError(exception.Message));
        }

        await productRepository.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}

public record AddValueToProductPropertyCommand(Guid ProductId, Guid PropertyId, Guid ValueId);
