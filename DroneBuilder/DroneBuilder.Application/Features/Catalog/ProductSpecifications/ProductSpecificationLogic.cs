using DroneBuilder.Application.Features.Catalog.ProductSpecifications.Models;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.ProductSpecifications;

internal static class ProductSpecificationLogic
{
    public static Result<Value?> ResolveOption(
        Property property,
        SpecificationValueInputModel model)
    {
        if (!model.ValueId.HasValue)
        {
            return Result.Ok<Value?>(null);
        }

        Value? option = property.Values.FirstOrDefault(value => value.Id == model.ValueId.Value);
        return option is null
            ? Result.Fail<Value?>(new ValidationError(
                $"Value with id {model.ValueId.Value} is not allowed for property '{property.Code}'."))
            : Result.Ok<Value?>(option);
    }

    public static bool HasDuplicate(
        IEnumerable<ProductPropertyValue> specifications,
        Guid propertyId,
        SpecificationValueInputModel model,
        Guid? excludedSpecificationId = null)
    {
        return specifications.Any(specification =>
            specification.Id != excludedSpecificationId &&
            specification.PropertyId == propertyId &&
            specification.ValueId == model.ValueId &&
            specification.TextValue == model.TextValue &&
            specification.NumericValue == model.NumericValue &&
            specification.MinNumericValue == model.MinNumericValue &&
            specification.MaxNumericValue == model.MaxNumericValue &&
            specification.BooleanValue == model.BooleanValue);
    }
    public static bool HasDuplicate(
        IEnumerable<ProductVariantPropertyValue> specifications,
        Guid propertyId,
        SpecificationValueInputModel model,
        Guid? excludedSpecificationId = null)
    {
        return specifications.Any(specification =>
            specification.Id != excludedSpecificationId &&
            specification.PropertyId == propertyId &&
            specification.ValueId == model.ValueId &&
            specification.TextValue == model.TextValue &&
            specification.NumericValue == model.NumericValue &&
            specification.MinNumericValue == model.MinNumericValue &&
            specification.MaxNumericValue == model.MaxNumericValue &&
            specification.BooleanValue == model.BooleanValue);
    }
}
