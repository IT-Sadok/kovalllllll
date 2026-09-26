using DroneBuilder.Application.Features.Builds.Compatibility;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Domain.Entities.Components;

namespace DroneBuilder.Application.Tests.Features.Builds;

public class CompatiblePartFilterTests
{
    private static BuildPart Part(ProductCategory category, ComponentSpec? spec, int quantity = 1)
        => new(Guid.NewGuid(), category.ToString(), category, spec, 10m, 10m, quantity);

    private static readonly BuildPart Frame = Part(ProductCategory.Frame, new FrameSpec
    {
        MaxPropSizeInch = 5.1m,
        FcMountPatterns = [MountPattern.M20x20, MountPattern.M30_5x30_5],
        MotorMountPatterns = [MountPattern.M16x16]
    });

    [Fact]
    public void Filter_ShouldKeepStacksThatFitTheFrameAndDropUnknownOnes()
    {
        // Arrange
        BuildPart fits = Part(ProductCategory.Stack, new StackSpec { MountPattern = MountPattern.M20x20, MinCells = 3, MaxCells = 6 });
        BuildPart tooSmall = Part(ProductCategory.Stack, new StackSpec { MountPattern = MountPattern.M16x16, MinCells = 3, MaxCells = 6 });
        BuildPart noSpec = Part(ProductCategory.Stack, null);

        // Act
        IReadOnlyList<Guid> compatible = CompatiblePartFilter.Filter([Frame], [fits, tooSmall, noSpec]);

        // Assert
        Assert.Equal([fits.ProductId], compatible);
    }

    [Fact]
    public void Filter_ShouldDropMotorsThatCannotTakeTheBatteryButKeepOnesWithOnlyWarnings()
    {
        // Arrange
        BuildPart battery = Part(ProductCategory.Battery, new BatterySpec { Cells = 6, CapacityMah = 1300, Connector = BatteryConnector.Xt60 });
        BuildPart sixCell = Part(ProductCategory.Motor,
            new MotorSpec { StatorSize = "2207", Kv = 1950, MountPattern = MountPattern.M16x16, MinCells = 4, MaxCells = 6 }, 4);
        BuildPart highKv = Part(ProductCategory.Motor,
            new MotorSpec { StatorSize = "2207", Kv = 2750, MountPattern = MountPattern.M16x16, MinCells = 4, MaxCells = 6 }, 4);
        BuildPart fourCell = Part(ProductCategory.Motor,
            new MotorSpec { StatorSize = "2207", Kv = 2450, MountPattern = MountPattern.M16x16, MinCells = 3, MaxCells = 4 }, 4);

        // Act
        IReadOnlyList<Guid> compatible = CompatiblePartFilter.Filter([Frame, battery], [sixCell, highKv, fourCell]);

        // Assert
        Assert.Equal([sixCell.ProductId, highKv.ProductId], compatible);
    }

    [Fact]
    public void Filter_ShouldHidePropsThatAreTooLargeOrMuchSmallerThanTheFrame()
    {
        // Arrange
        BuildPart fiveInch = Part(ProductCategory.Propeller, new PropellerSpec { DiameterInch = 5.1m });
        BuildPart threeInch = Part(ProductCategory.Propeller, new PropellerSpec { DiameterInch = 3m });
        BuildPart sevenInch = Part(ProductCategory.Propeller, new PropellerSpec { DiameterInch = 7m });

        // Act
        IReadOnlyList<Guid> compatible = CompatiblePartFilter.Filter([Frame], [fiveInch, threeInch, sevenInch]);

        // Assert
        Assert.Equal([fiveInch.ProductId], compatible);
    }

    [Fact]
    public void Filter_ShouldKeepOnlyReceiversThatSpeakTheRadiosProtocol()
    {
        // Arrange
        BuildPart radio = Part(ProductCategory.Radio, new RadioSpec { Protocol = RadioProtocol.ExpressLrs });
        BuildPart elrs = Part(ProductCategory.Receiver, new ReceiverSpec { Protocol = RadioProtocol.ExpressLrs });
        BuildPart crossfire = Part(ProductCategory.Receiver, new ReceiverSpec { Protocol = RadioProtocol.Crossfire });

        // Act
        IReadOnlyList<Guid> compatible = CompatiblePartFilter.Filter([Frame, radio], [elrs, crossfire]);

        // Assert
        Assert.Equal([elrs.ProductId], compatible);
    }

    [Fact]
    public void Filter_WhenCandidateIsAlreadySelected_ShouldCheckItOnlyOnce()
    {
        // Arrange
        BuildPart stack = Part(ProductCategory.Stack, new StackSpec { MountPattern = MountPattern.M20x20, MinCells = 3, MaxCells = 6 });

        // Act
        IReadOnlyList<Guid> compatible = CompatiblePartFilter.Filter([Frame, stack], [stack]);

        // Assert
        Assert.Equal([stack.ProductId], compatible);
    }
}
