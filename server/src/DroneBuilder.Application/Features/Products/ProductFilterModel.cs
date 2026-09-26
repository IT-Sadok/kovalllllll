using DroneBuilder.Domain.Entities;
using DroneBuilder.Domain.Entities.Components;

namespace DroneBuilder.Application.Features.Products;

public class ProductFilterModel
{
    public string? Name { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public ProductCategory? Category { get; set; }
    public bool? CollapseVariants { get; set; }
    public string? Manufacturer { get; set; }
    public bool? InStock { get; set; }
    public ProductSort? Sort { get; set; }
    public int? Cells { get; set; }
    public MountPattern? MountPattern { get; set; }
    public VideoSystem? VideoSystem { get; set; }
    public int? KvMin { get; set; }
    public int? KvMax { get; set; }
    public decimal? PropSizeInch { get; set; }
    public int? CapacityMin { get; set; }
    public int? CapacityMax { get; set; }
    public BatteryConnector? BatteryConnector { get; set; }
    public RadioProtocol? Protocol { get; set; }
    public RfConnector? RfConnector { get; set; }
}
