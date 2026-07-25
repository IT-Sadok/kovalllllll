namespace DroneBuilder.Application.Models.ProductModels;

public class ValueModel
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public decimal? NumericValue { get; set; }
    public bool? BooleanValue { get; set; }
    public ICollection<string> Aliases { get; set; } = [];
}
