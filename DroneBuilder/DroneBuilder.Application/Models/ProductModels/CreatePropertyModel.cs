namespace DroneBuilder.Application.Models.ProductModels;

public class CreatePropertyModel
{
    public string? Code { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = "Option";
    public Guid? UnitDefinitionId { get; set; }
    public bool IsFilterable { get; set; } = true;
    public bool IsCompatibilityRelevant { get; set; }
    public bool AllowsMultipleValues { get; set; } = true;
    public ICollection<string> Aliases { get; set; } = [];
    public ICollection<CreateValueModel> Values { get; set; } = [];
}
