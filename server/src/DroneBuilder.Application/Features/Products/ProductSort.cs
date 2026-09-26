using System.Text.Json.Serialization;

namespace DroneBuilder.Application.Features.Products;

[JsonConverter(typeof(JsonStringEnumConverter<ProductSort>))]
public enum ProductSort
{
    Name = 0,
    PriceAsc = 1,
    PriceDesc = 2
}
