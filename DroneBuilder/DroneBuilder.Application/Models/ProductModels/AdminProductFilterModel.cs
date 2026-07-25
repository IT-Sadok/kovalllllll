namespace DroneBuilder.Application.Models.ProductModels;

public sealed class AdminProductFilterModel : ProductFilterModel
{
    public string? PublicationStatus { get; set; }
    public bool IncludeArchived { get; set; }
}
