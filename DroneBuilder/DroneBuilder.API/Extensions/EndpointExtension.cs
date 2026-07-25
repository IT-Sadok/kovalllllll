using DroneBuilder.API.Features.Auth;
using DroneBuilder.API.Features.Cart;
using DroneBuilder.API.Features.Catalog.Compatibility;
using DroneBuilder.API.Features.Catalog.Images;
using DroneBuilder.API.Features.Catalog.Imports;
using DroneBuilder.API.Features.Catalog.Metadata;
using DroneBuilder.API.Features.Catalog.Products;
using DroneBuilder.API.Features.Catalog.ProductSpecifications;
using DroneBuilder.API.Features.Catalog.ProductVariants;
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
            .MapCatalogMetadataEndpoints()
            .MapCatalogImportEndpoints()
            .MapCompatibilityEndpoints()
            .MapProductSpecificationEndpoints()
            .MapProductVariantEndpoints()
            .MapPropertyEndpoints()
            .MapValueEndpoints()
            .MapImageEndpoints()
            .MapCartEndpoints()
            .MapWarehouseEndpoints()
            .MapOrderEndpoints();

        return app;
    }
}
