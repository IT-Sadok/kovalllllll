using DroneBuilder.API.Features.Carts;
using DroneBuilder.API.Features.Images;
using DroneBuilder.API.Features.Orders;
using DroneBuilder.API.Features.Products;
using DroneBuilder.API.Features.Properties;
using DroneBuilder.API.Features.Users;
using DroneBuilder.API.Features.Values;
using DroneBuilder.API.Features.Warehouses;

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
            .MapOrderEndpoints()
            .MapPaymentEndpoints();

        return app;
    }
}
