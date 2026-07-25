namespace DroneBuilder.Domain.Entities;

public class CompatibilityRule : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid LeftComponentTypeId { get; set; }
    public ComponentType? LeftComponentType { get; set; }
    public Guid LeftPropertyId { get; set; }
    public Property? LeftProperty { get; set; }
    public Guid RightComponentTypeId { get; set; }
    public ComponentType? RightComponentType { get; set; }
    public Guid RightPropertyId { get; set; }
    public Property? RightProperty { get; set; }
    public CompatibilityOperator Operator { get; set; }
    public string? FailureMessage { get; set; }
    public bool IsActive { get; set; } = true;

    public void ValidateDefinition()
    {
        ComponentType leftType = LeftComponentType
            ?? throw new InvalidOperationException("Left component type metadata is required.");
        ComponentType rightType = RightComponentType
            ?? throw new InvalidOperationException("Right component type metadata is required.");
        Property leftProperty = LeftProperty
            ?? throw new InvalidOperationException("Left property metadata is required.");
        Property rightProperty = RightProperty
            ?? throw new InvalidOperationException("Right property metadata is required.");

        if (!leftType.Properties.Any(rule => rule.PropertyId == leftProperty.Id))
        {
            throw new InvalidOperationException(
                $"Property '{leftProperty.Code}' is not allowed for component type '{leftType.Code}'.");
        }

        if (!rightType.Properties.Any(rule => rule.PropertyId == rightProperty.Id))
        {
            throw new InvalidOperationException(
                $"Property '{rightProperty.Code}' is not allowed for component type '{rightType.Code}'.");
        }

        bool leftNumeric = leftProperty.DataType is SpecificationDataType.Number or SpecificationDataType.NumericRange ||
                           leftProperty.DataType == SpecificationDataType.Option &&
                           leftProperty.Values.Any(value => value.NumericValue.HasValue);
        bool rightNumeric = rightProperty.DataType is SpecificationDataType.Number or SpecificationDataType.NumericRange ||
                            rightProperty.DataType == SpecificationDataType.Option &&
                            rightProperty.Values.Any(value => value.NumericValue.HasValue);

        bool numericOperator = Operator is CompatibilityOperator.LeftLessThanOrEqualRight or
            CompatibilityOperator.LeftGreaterThanOrEqualRight or
            CompatibilityOperator.LeftContainedByRightRange or
            CompatibilityOperator.RightContainedByLeftRange or
            CompatibilityOperator.RangesOverlap ||
            Operator == CompatibilityOperator.Equals && (leftNumeric || rightNumeric);
        if (numericOperator)
        {
            if (!leftNumeric || !rightNumeric)
            {
                throw new InvalidOperationException("The selected compatibility operator requires numeric properties.");
            }

            string? leftDimension = leftProperty.UnitDefinition?.Dimension;
            string? rightDimension = rightProperty.UnitDefinition?.Dimension;
            if (string.IsNullOrWhiteSpace(leftDimension) ||
                string.IsNullOrWhiteSpace(rightDimension) ||
                !string.Equals(leftDimension, rightDimension, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Numeric compatibility properties must use units from the same dimension.");
            }
        }

        if (Operator == CompatibilityOperator.AnyOptionMatches &&
            (leftProperty.DataType != SpecificationDataType.Option ||
             rightProperty.DataType != SpecificationDataType.Option))
        {
            throw new InvalidOperationException("AnyOptionMatches requires Option properties on both sides.");
        }
    }
}
