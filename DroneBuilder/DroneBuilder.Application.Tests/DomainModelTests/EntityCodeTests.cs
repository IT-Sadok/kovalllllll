using DroneBuilder.Application.Common;

namespace DroneBuilder.Application.Tests.DomainModelTests;

public class EntityCodeTests
{
    [Theory]
    [InlineData("Battery Voltage", "battery-voltage")]
    [InlineData("  Motor KV  ", "motor-kv")]
    [InlineData("20×20 mm", "20-20-mm")]
    public void FromName_ReturnsStableImportFriendlyCode(string input, string expected)
    {
        Assert.Equal(expected, EntityCode.FromName(input));
    }
}
