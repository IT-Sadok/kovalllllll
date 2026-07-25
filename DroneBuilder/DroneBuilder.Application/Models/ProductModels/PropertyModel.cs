namespace DroneBuilder.Application.Models.ProductModels;

public class PropertyModel
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public Guid? UnitDefinitionId { get; set; }
    public bool IsFilterable { get; set; }
    public bool IsCompatibilityRelevant { get; set; }
    public bool AllowsMultipleValues { get; set; }
    public ICollection<string> Aliases { get; set; } = [];
    public ICollection<ValueModel> Values { get; set; } = [];
}
