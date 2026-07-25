namespace DroneBuilder.Application.Models.ProductModels;

public class UpdateValueModel
{
    public string? Code { get; set; }
    public string? Text { get; set; }
    public decimal? NumericValue { get; set; }
    public bool ClearNumericValue { get; set; }
    public bool? BooleanValue { get; set; }
    public bool ClearBooleanValue { get; set; }
    public ICollection<string>? Aliases { get; set; }
}
