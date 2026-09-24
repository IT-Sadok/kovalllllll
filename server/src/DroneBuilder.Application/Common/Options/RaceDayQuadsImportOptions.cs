namespace DroneBuilder.Application.Common.Options;

public class RaceDayQuadsImportOptions
{
    public string BaseUrl { get; set; } = "https://www.racedayquads.com/";
    public int MaxProductsPerCategory { get; set; } = 20;
    public int MaxVariantsPerProduct { get; set; } = 6;
    public int RequestDelayMs { get; set; } = 1000;
}
