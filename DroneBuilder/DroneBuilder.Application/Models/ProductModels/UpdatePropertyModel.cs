namespace DroneBuilder.Application.Models.ProductModels;

public class UpdatePropertyModel
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? DataType { get; set; }
    public Guid? UnitDefinitionId { get; set; }
    public bool ClearUnitDefinition { get; set; }
    public bool? IsFilterable { get; set; }
    public bool? IsCompatibilityRelevant { get; set; }
    public bool? AllowsMultipleValues { get; set; }
    public ICollection<string>? Aliases { get; set; }
}
