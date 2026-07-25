using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Tests.DomainModelTests;

public class SpecificationModelTests
{
    [Fact]
    public void Validate_NumberWithCanonicalUnit_Succeeds()
    {
        var property = new Property
        {
            Code = "max-current",
            DataType = SpecificationDataType.Number,
            UnitDefinitionId = Guid.NewGuid(),
            IsCompatibilityRelevant = true
        };
        var specification = new ProductPropertyValue
        {
            Property = property,
            PropertyId = property.Id,
            NumericValue = 30m
        };

        specification.Validate();
    }

    [Fact]
    public void Validate_NumberStoredAsText_Throws()
    {
        var property = new Property
        {
            Code = "max-current",
            DataType = SpecificationDataType.Number,
            UnitDefinitionId = Guid.NewGuid()
        };
        var specification = new ProductPropertyValue
        {
            Property = property,
            TextValue = "30A"
        };

        InvalidOperationException exception =
            Assert.Throws<InvalidOperationException>(specification.Validate);

        Assert.Contains("does not match", exception.Message);
    }

    [Fact]
    public void Validate_MultipleRepresentations_Throws()
    {
        var property = new Property
        {
            Code = "weight",
            DataType = SpecificationDataType.Number,
            UnitDefinitionId = Guid.NewGuid()
        };
        var specification = new ProductPropertyValue
        {
            Property = property,
            NumericValue = 20m,
            TextValue = "20g"
        };

        InvalidOperationException exception =
            Assert.Throws<InvalidOperationException>(specification.Validate);

        Assert.Contains("exactly one", exception.Message);
    }

    [Fact]
    public void AddSpecification_WhenSingleValuePropertyAlreadyExists_Throws()
    {
        var property = new Property
        {
            Code = "motor-kv",
            DataType = SpecificationDataType.Number,
            UnitDefinitionId = Guid.NewGuid(),
            AllowsMultipleValues = false
        };
        var product = new Product();
        product.AddSpecification(new ProductPropertyValue
        {
            Property = property,
            PropertyId = property.Id,
            NumericValue = 1750m
        });

        Assert.Throws<InvalidOperationException>(() =>
            product.AddSpecification(new ProductPropertyValue
            {
                Property = property,
                PropertyId = property.Id,
                NumericValue = 1950m
            }));
    }

    [Fact]
    public void ValidateForPublication_WithoutDefaultVariant_Throws()
    {
        var product = new Product { IsActive = true };

        InvalidOperationException exception =
            Assert.Throws<InvalidOperationException>(product.ValidateForPublication);

        Assert.Contains("exactly one active default variant", exception.Message);
    }

    [Fact]
    public void ComponentTypeRule_PreventsProductLevelVariantProperty()
    {
        var property = new Property
        {
            Code = "motor-kv",
            DataType = SpecificationDataType.Number,
            UnitDefinitionId = Guid.NewGuid()
        };
        var componentType = new ComponentType
        {
            Code = "motor",
            Properties =
            [
                new ComponentTypeProperty
                {
                    PropertyId = property.Id,
                    IsVariantSpecific = true
                }
            ]
        };
        var product = new Product { ComponentType = componentType };

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            product.AddSpecification(new ProductPropertyValue
            {
                Property = property,
                PropertyId = property.Id,
                NumericValue = 1750m
            }));

        Assert.Contains("variant level", exception.Message);
    }

    [Theory]
    [InlineData("20x20", 20, 20, "mm")]
    [InlineData("20×20 mm", 20, 20, "mm")]
    [InlineData("20mm x 20mm", 20, 20, "mm")]
    [InlineData("1.5 x 2 inch", 1.5, 2, "inch")]
    public void TryParseDimensionPair_NormalizesSupportedFormats(
        string input,
        decimal expectedWidth,
        decimal expectedHeight,
        string expectedUnit)
    {
        bool parsed = SpecificationAliasNormalizer.TryParseDimensionPair(
            input,
            out ParsedDimensionPair dimensions);

        Assert.True(parsed);
        Assert.Equal(expectedWidth, dimensions.Width);
        Assert.Equal(expectedHeight, dimensions.Height);
        Assert.Equal(expectedUnit, dimensions.UnitAlias);
    }
}
