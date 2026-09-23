namespace DroneBuilder.Application.Features.Values;

public class CreateValueModel
{
    public string Text { get; set; } = string.Empty;
    public Guid PropertyId { get; set; }
}
