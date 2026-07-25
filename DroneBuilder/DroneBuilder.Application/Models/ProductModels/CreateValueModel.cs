namespace DroneBuilder.Application.Models.ProductModels;

public class CreateValueModel
{
    public string? Code { get; set; }
    public string Text { get; set; } = string.Empty;
    public Guid PropertyId { get; set; }
    public decimal? NumericValue { get; set; }
    public bool? BooleanValue { get; set; }
    public ICollection<string> Aliases { get; set; } = [];
}
