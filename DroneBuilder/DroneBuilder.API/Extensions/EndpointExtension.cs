using DroneBuilder.API.Features.Auth;
using DroneBuilder.API.Features.Cart;
using DroneBuilder.API.Features.Catalog.Images;
using DroneBuilder.API.Features.Catalog.Products;
using DroneBuilder.API.Features.Catalog.Properties;
using DroneBuilder.API.Features.Catalog.Values;
using DroneBuilder.API.Features.Inventory;
using DroneBuilder.API.Features.Orders;

namespace DroneBuilder.API.Extensions;

public static class EndpointExtension
{
    public static WebApplication MapApplicationEndpoints(this WebApplication app)
    {
        app.MapUserEndpoints()
            .MapProductEndpoints()
            .MapPropertyEndpoints()
            .MapValueEndpoints()
            .MapImageEndpoints()
            .MapCartEndpoints()
            .MapWarehouseEndpoints()
            .MapOrderEndpoints();

        return app;
    }
}
