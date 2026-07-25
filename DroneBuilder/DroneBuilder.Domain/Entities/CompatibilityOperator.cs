namespace DroneBuilder.Domain.Entities;

public enum CompatibilityOperator
{
    Equals = 0,
    LeftLessThanOrEqualRight = 1,
    LeftGreaterThanOrEqualRight = 2,
    LeftContainedByRightRange = 3,
    RightContainedByLeftRange = 4,
    RangesOverlap = 5,
    AnyOptionMatches = 6
}
