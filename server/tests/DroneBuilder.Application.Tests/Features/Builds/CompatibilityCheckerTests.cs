using DroneBuilder.Application.Features.Builds.Compatibility;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Domain.Entities.Components;

namespace DroneBuilder.Application.Tests.Features.Builds;

public class CompatibilityCheckerTests
{
    private static BuildPart Part(ProductCategory category, ComponentSpec? spec, decimal? weight, int quantity = 1,
        string? name = null)
        => new(Guid.NewGuid(), name ?? category.ToString(), category, spec, 10m, weight, quantity);

    private static BuildPart Frame(FrameSpec? spec = null) => Part(ProductCategory.Frame, spec ?? new FrameSpec
    {
        MaxPropSizeInch = 5.1m,
        FcMountPatterns = [MountPattern.M30_5x30_5, MountPattern.M20x20],
        MotorMountPatterns = [MountPattern.M16x16],
        CameraWidthMm = 19
    }, 118m);

    private static BuildPart Motor(MotorSpec? spec = null, int quantity = 4) => Part(ProductCategory.Motor, spec ?? new MotorSpec
    {
        StatorSize = "2207",
        Kv = 1950,
        MountPattern = MountPattern.M16x16,
        MinCells = 4,
        MaxCells = 6,
        MaxCurrentA = 45m,
        ShaftMm = 5m,
        MaxThrustGrams = 1700
    }, 32.9m, quantity);

    private static BuildPart Prop(PropellerSpec? spec = null) => Part(ProductCategory.Propeller, spec ?? new PropellerSpec
    {
        DiameterInch = 5.1m,
        PitchInch = 4.3m,
        BladeCount = 3,
        HubMm = 5m
    }, 4.4m);

    private static BuildPart Stack(StackSpec? spec = null) => Part(ProductCategory.Stack, spec ?? new StackSpec
    {
        MountPattern = MountPattern.M20x20,
        MinCells = 3,
        MaxCells = 6,
        ContinuousCurrentA = 55m,
        BatteryConnector = BatteryConnector.Xt60
    }, 19m);

    private static BuildPart Battery(BatterySpec? spec = null) => Part(ProductCategory.Battery, spec ?? new BatterySpec
    {
        Cells = 6,
        CapacityMah = 1300,
        CRating = 120,
        Connector = BatteryConnector.Xt60
    }, 215m);

    private static BuildPart Vtx(VideoTransmitterSpec? spec = null) => Part(ProductCategory.VideoTransmitter, spec ?? new VideoTransmitterSpec
    {
        VideoSystem = VideoSystem.Analog,
        AntennaConnector = RfConnector.Ufl,
        MountPattern = MountPattern.M20x20
    }, 5m);

    private static BuildPart Camera(CameraSpec? spec = null)
        => Part(ProductCategory.Camera, spec ?? new CameraSpec { VideoSystem = VideoSystem.Analog, WidthMm = 19 }, 6m);

    private static BuildPart Receiver()
        => Part(ProductCategory.Receiver, new ReceiverSpec { Protocol = RadioProtocol.ExpressLrs }, 1m);

    private static BuildPart Antenna(RfConnector connector = RfConnector.Ufl)
        => Part(ProductCategory.Antenna, new AntennaSpec { Connector = connector }, 3m);

    private static List<BuildPart> ValidBuild()
        => [Frame(), Motor(), Prop(), Stack(), Battery(), Vtx(), Camera(), Receiver(), Antenna()];

    private static IReadOnlyList<CompatibilityIssue> Check(IEnumerable<BuildPart> parts)
        => CompatibilityChecker.Check(new BuildParts(parts.ToList()));

    private static List<BuildPart> Replace(ProductCategory category, BuildPart replacement)
        => ValidBuild().Where(p => p.Category != category).Append(replacement).ToList();

    [Fact]
    public void Check_WhenBuildIsBalanced_ShouldOnlyReportTheWeightLimit()
    {
        // Act
        IReadOnlyList<CompatibilityIssue> issues = Check(ValidBuild());

        // Assert
        CompatibilityIssue issue = Assert.Single(issues);
        Assert.Equal("over_250g", issue.Code);
        Assert.Equal(IssueSeverity.Info, issue.Severity);
    }

    [Fact]
    public void Check_WhenBuildIsEmpty_ShouldAskForEveryRequiredPart()
    {
        // Act
        IReadOnlyList<CompatibilityIssue> issues = Check([]);

        // Assert
        Assert.Equal(
            ["missing_frame", "missing_motors", "missing_propellers", "missing_fc_esc", "missing_battery"],
            issues.Where(i => i.Severity == IssueSeverity.Error).Select(i => i.Code));
    }

    [Fact]
    public void Check_WhenThreeMotors_ShouldReportMotorCount()
    {
        // Act
        IReadOnlyList<CompatibilityIssue> issues = Check(Replace(ProductCategory.Motor, Motor(quantity: 3)));

        // Assert
        Assert.Contains(issues, i => i is { Code: "motor_count", Severity: IssueSeverity.Error });
    }

    [Fact]
    public void Check_WhenSeparateFcWithoutEsc_ShouldAskForEsc()
    {
        // Arrange
        List<BuildPart> parts = ValidBuild().Where(p => p.Category != ProductCategory.Stack).ToList();
        parts.Add(Part(ProductCategory.FlightController,
            new FlightControllerSpec { MountPattern = MountPattern.M20x20, MinCells = 3, MaxCells = 6 }, 8m));

        // Act & Assert
        Assert.Contains(Check(parts), i => i.Code == "missing_esc");
    }

    [Fact]
    public void Check_WhenStackAndSeparateFc_ShouldWarnAboutDuplicate()
    {
        // Arrange
        List<BuildPart> parts = ValidBuild();
        parts.Add(Part(ProductCategory.FlightController,
            new FlightControllerSpec { MountPattern = MountPattern.M20x20, MinCells = 3, MaxCells = 6 }, 8m));

        // Act & Assert
        Assert.Contains(Check(parts), i => i is { Code: "stack_duplicates_fc_esc", Severity: IssueSeverity.Warning });
    }

    [Fact]
    public void Check_WhenPropIsTooLargeForFrame_ShouldReportError()
    {
        // Act
        IReadOnlyList<CompatibilityIssue> issues =
            Check(Replace(ProductCategory.Propeller, Prop(new PropellerSpec { DiameterInch = 7m, HubMm = 5m })));

        // Assert
        CompatibilityIssue issue = Assert.Single(issues, i => i.Code == "prop_too_large");
        Assert.Equal(IssueSeverity.Error, issue.Severity);
        Assert.Contains("(7\") is larger than the 5.1\" props", issue.Message);
    }

    [Theory]
    [InlineData(3, true)]
    [InlineData(4, true)]
    [InlineData(4.5, false)]
    public void Check_WhenPropIsMuchSmallerThanFrame_ShouldOnlyWarn(double diameter, bool expected)
    {
        // Act
        IReadOnlyList<CompatibilityIssue> issues = Check(Replace(ProductCategory.Propeller,
            Prop(new PropellerSpec { DiameterInch = (decimal)diameter, HubMm = 5m })));

        // Assert
        Assert.Equal(expected, issues.Any(i => i is { Code: "prop_undersized", Severity: IssueSeverity.Warning }));
        Assert.DoesNotContain(issues, i => i.Code.StartsWith("prop_") && i.Severity == IssueSeverity.Error);
    }

    [Fact]
    public void Check_WhenStackDoesNotFitFrame_ShouldReportError()
    {
        // Act
        IReadOnlyList<CompatibilityIssue> issues = Check(Replace(ProductCategory.Stack, Stack(new StackSpec
        {
            MountPattern = MountPattern.M16x16,
            MinCells = 3,
            MaxCells = 6,
            ContinuousCurrentA = 55m,
            BatteryConnector = BatteryConnector.Xt60
        })));

        // Assert
        CompatibilityIssue issue = Assert.Single(issues, i => i.Code == "stack_mount");
        Assert.EndsWith("takes 30.5x30.5 mm or 20x20 mm.", issue.Message);
    }

    [Fact]
    public void Check_WhenMotorBoltPatternDiffers_ShouldReportError()
    {
        // Act
        IReadOnlyList<CompatibilityIssue> issues = Check(Replace(ProductCategory.Motor, Motor(new MotorSpec
        {
            StatorSize = "2306",
            Kv = 1750,
            MountPattern = MountPattern.M19x19,
            MinCells = 4,
            MaxCells = 6,
            MaxCurrentA = 45m,
            ShaftMm = 5m
        })));

        // Assert
        Assert.Contains(issues, i => i is { Code: "motor_mount", Severity: IssueSeverity.Error });
    }

    [Fact]
    public void Check_WhenFrameMotorMountIsUnknown_ShouldSayItCouldNotCheck()
    {
        // Act
        IReadOnlyList<CompatibilityIssue> issues = Check(Replace(ProductCategory.Frame, Frame(new FrameSpec
        {
            MaxPropSizeInch = 5.1m,
            FcMountPatterns = [MountPattern.M20x20],
            MotorMountPatterns = [],
            CameraWidthMm = 19
        })));

        // Assert
        Assert.Contains(issues, i => i is { Code: "motor_mount_unknown", Severity: IssueSeverity.Info });
        Assert.DoesNotContain(issues, i => i.Code == "motor_mount");
    }

    [Fact]
    public void Check_WhenBatteryHasTooManyCells_ShouldReportEveryPart()
    {
        // Act
        IReadOnlyList<CompatibilityIssue> issues = Check(Replace(ProductCategory.Battery, Battery(new BatterySpec
        {
            Cells = 8,
            CapacityMah = 1100,
            Connector = BatteryConnector.Xt60
        })));

        // Assert
        Assert.Equal(2, issues.Count(i => i.Code == "battery_cells"));
        Assert.Contains(issues, i => i.Message == "Battery is 8S, but Motor is rated for 4-6S.");
    }

    [Fact]
    public void Check_WhenCameraAndVtxUseDifferentSystems_ShouldReportError()
    {
        // Act
        IReadOnlyList<CompatibilityIssue> issues = Check(Replace(ProductCategory.Camera,
            Camera(new CameraSpec { VideoSystem = VideoSystem.DjiO4, WidthMm = 19 })));

        // Assert
        CompatibilityIssue issue = Assert.Single(issues, i => i.Code == "video_system");
        Assert.Equal("Camera is DJI O4, but VideoTransmitter is Analog.", issue.Message);
    }

    [Fact]
    public void Check_WhenEscHasNoHeadroom_ShouldWarn()
    {
        // Act
        IReadOnlyList<CompatibilityIssue> issues = Check(Replace(ProductCategory.Stack, Stack(new StackSpec
        {
            MountPattern = MountPattern.M20x20,
            MinCells = 3,
            MaxCells = 6,
            ContinuousCurrentA = 45m,
            BatteryConnector = BatteryConnector.Xt60
        })));

        // Assert
        Assert.Contains(issues, i => i is { Code: "esc_current", Severity: IssueSeverity.Warning });
    }

    [Fact]
    public void Check_WhenEscCurrentIsUnknown_ShouldSayItCouldNotCheck()
    {
        // Act
        IReadOnlyList<CompatibilityIssue> issues = Check(Replace(ProductCategory.Stack, Stack(new StackSpec
        {
            MountPattern = MountPattern.M20x20,
            MinCells = 3,
            MaxCells = 6,
            BatteryConnector = BatteryConnector.Xt60
        })));

        // Assert
        Assert.Contains(issues, i => i is { Code: "esc_current_unknown", Severity: IssueSeverity.Info });
    }

    [Fact]
    public void Check_WhenBatteryPlugDiffersFromLead_ShouldWarnAboutAdapter()
    {
        // Act
        IReadOnlyList<CompatibilityIssue> issues = Check(Replace(ProductCategory.Battery, Battery(new BatterySpec
        {
            Cells = 6,
            CapacityMah = 1300,
            Connector = BatteryConnector.Xt30
        })));

        // Assert
        CompatibilityIssue issue = Assert.Single(issues, i => i.Code == "battery_connector");
        Assert.StartsWith("Battery has a XT30 plug, but Stack has a XT60 lead", issue.Message);
    }

    [Fact]
    public void Check_WhenAntennaConnectorDiffers_ShouldWarn()
    {
        // Act
        IReadOnlyList<CompatibilityIssue> issues =
            Check(Replace(ProductCategory.Antenna, Antenna(RfConnector.Mmcx)));

        // Assert
        CompatibilityIssue issue = Assert.Single(issues, i => i.Code == "antenna_connector");
        Assert.Contains("MMCX connector, but VideoTransmitter expects U.FL", issue.Message);
    }

    [Fact]
    public void Check_WhenPropHubDoesNotMatchShaft_ShouldReportError()
    {
        // Act
        IReadOnlyList<CompatibilityIssue> issues = Check(Replace(ProductCategory.Propeller,
            Prop(new PropellerSpec { DiameterInch = 5.1m, HubMm = 1.5m })));

        // Assert
        Assert.Contains(issues, i => i is { Code: "prop_hub", Severity: IssueSeverity.Error });
    }

    [Fact]
    public void Check_WhenKvIsTooHighForSixCells_ShouldWarn()
    {
        // Act
        IReadOnlyList<CompatibilityIssue> issues = Check(Replace(ProductCategory.Motor, Motor(new MotorSpec
        {
            StatorSize = "2207",
            Kv = 2450,
            MountPattern = MountPattern.M16x16,
            MinCells = 4,
            MaxCells = 6,
            MaxCurrentA = 45m,
            ShaftMm = 5m
        })));

        // Assert
        CompatibilityIssue issue = Assert.Single(issues, i => i.Code == "kv_too_high");
        Assert.StartsWith("2450 KV on 6S with 5.1\" props", issue.Message);
    }

    [Fact]
    public void Check_WhenComponentHasNoSpec_ShouldSayItCouldNotCheck()
    {
        // Act
        IReadOnlyList<CompatibilityIssue> issues = Check(Replace(ProductCategory.Camera,
            Part(ProductCategory.Camera, null, 6m, name: "Mystery Cam")));

        // Assert
        CompatibilityIssue issue = Assert.Single(issues, i => i.Code == "missing_spec");
        Assert.Equal("Mystery Cam has no specification yet, so it could not be checked.", issue.Message);
    }

    [Fact]
    public void Check_ShouldOrderErrorsBeforeWarningsBeforeInfo()
    {
        // Act
        IReadOnlyList<CompatibilityIssue> issues = Check([Motor(quantity: 2)]);

        // Assert
        Assert.Equal(issues.OrderBy(i => i.Severity).Select(i => i.Code), issues.Select(i => i.Code));
    }
}
