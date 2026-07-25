namespace DroneBuilder.API.Endpoints.Routes;

public abstract class ApiRoutes
{
    private const string Base = "/api";

    public static class Users
    {
        private const string BaseRoute = Base + "/users";
        public const string SignUp = BaseRoute + "/sign-up";
        public const string SignIn = BaseRoute + "/sign-in";
    }

    public static class Products
    {
        private const string BaseRoute = Base + "/products";
        public const string GetAll = BaseRoute;
        public const string GetById = BaseRoute + "/{productId}";
        public const string Create = BaseRoute;
        public const string Update = BaseRoute + "/{productId}";
        public const string Delete = BaseRoute + "/{productId}";
        public const string GetCategories = BaseRoute + "/categories";
        public const string GetPropertiesByProductId = BaseRoute + "/{productId}/properties";
        public const string AssignValueToProductProperty = BaseRoute + "/{productId}/properties/{propertyId}/values/{valueId}";
        public const string RemoveValueFromProductProperty = BaseRoute + "/{productId}/properties/{propertyId}/values/{valueId}";
        public const string RemovePropertyFromProduct = BaseRoute + "/{productId}/properties/{propertyId}";
        public const string AssignComponentType = BaseRoute + "/{productId}/component-type";
        public const string Specifications = BaseRoute + "/{productId}/specifications";
        public const string SpecificationById = Specifications + "/{specificationId}";
        public const string Publish = BaseRoute + "/{productId}/publish";
        public const string MoveToDraft = BaseRoute + "/{productId}/draft";
    }

    public static class AdminProducts
    {
        private const string BaseRoute = Base + "/admin/products";
        public const string GetAll = BaseRoute;
        public const string GetById = BaseRoute + "/{productId}";
        public const string Variants = GetById + "/variants";
        public const string Specifications = GetById + "/specifications";
    }

    public static class AdminComponentTypes
    {
        private const string BaseRoute = Base + "/admin/component-types";
        public const string GetAll = BaseRoute;
        public const string GetById = BaseRoute + "/{componentTypeId}";
    }

    public static class ProductVariants
    {
        private const string BaseRoute = Base + "/products/{productId}/variants";
        public const string GetAll = BaseRoute;
        public const string Create = BaseRoute;
        public const string ById = BaseRoute + "/{variantId}";
        public const string Specifications = ById + "/specifications";
        public const string SpecificationById = Specifications + "/{specificationId}";
    }
    public static class ComponentTypes
    {
        private const string BaseRoute = Base + "/component-types";
        public const string GetAll = BaseRoute;
        public const string Create = BaseRoute;
        public const string Update = BaseRoute + "/{componentTypeId}";
        public const string Delete = BaseRoute + "/{componentTypeId}";
        public const string GetProperties = BaseRoute + "/{componentTypeId}/properties";
        public const string PropertyRule = BaseRoute + "/{componentTypeId}/properties/{propertyId}";
    }

    public static class Units
    {
        private const string BaseRoute = Base + "/units";
        public const string GetAll = BaseRoute;
        public const string Create = BaseRoute;
        public const string Update = BaseRoute + "/{unitId}";
        public const string Delete = BaseRoute + "/{unitId}";
    }

    public static class Properties
    {
        private const string BaseRoute = Base + "/properties";
        public const string Create = BaseRoute;
        public const string Update = BaseRoute + "/{propertyId}";
        public const string Delete = BaseRoute + "/{propertyId}";
        public const string GetAll = BaseRoute;
        public const string GetValuesByPropertyId = BaseRoute + "/{propertyId}/values";
        public const string AssignValueToProperty = BaseRoute + "/{propertyId}/values/{valueId}";
        public const string RemoveValueFromProperty = BaseRoute + "/{propertyId}/values/{valueId}";
    }

    public static class Values
    {
        private const string BaseRoute = Base + "/values";
        public const string Create = BaseRoute;
        public const string Update = BaseRoute + "/{valueId}";
        public const string Delete = BaseRoute + "/{valueId}";
        public const string GetAll = BaseRoute;
        public const string GetById = BaseRoute + "/{valueId}";
    }

    public static class Compatibility
    {
        private const string BaseRoute = Base + "/compatibility";
        public const string Check = BaseRoute + "/check";
        public const string Rules = BaseRoute + "/rules";
        public const string RuleById = Rules + "/{ruleId}";
    }
    public static class Imports
    {
        private const string BaseRoute = Base + "/import-sources";
        public const string Sources = BaseRoute;
        public const string SourceById = BaseRoute + "/{sourceId}";
        public const string Batches = SourceById + "/batches";
        public const string Items = Batches + "/{batchId}/items";
        public const string PropertyAliases = SourceById + "/properties/{propertyId}/aliases";
        public const string ValueAliases = SourceById + "/values/{valueId}/aliases";
        public const string ProductReferences = Base + "/products/{productId}/external-references";
        public const string ProductReferenceBySource = ProductReferences + "/sources/{sourceId}";
        public const string ProductReferenceById = ProductReferences + "/{referenceId}";
        public const string VariantReferences = Base + "/products/{productId}/variants/{variantId}/external-references";
        public const string VariantReferenceBySource = VariantReferences + "/sources/{sourceId}";
        public const string VariantReferenceById = VariantReferences + "/{referenceId}";
    }
    public static class Images
    {
        private const string BaseRoute = Base + "/images";
        public const string Upload = BaseRoute + "/upload";
        public const string Delete = BaseRoute + "/{imageId}";
        public const string GetById = BaseRoute + "/{imageId}";
        public const string GetAll = BaseRoute;
        public const string GetImagesByProductId = BaseRoute + "/product/{productId}";
        public const string SetPrimary = BaseRoute + "/{imageId}/set-primary";
    }

    public static class Cart
    {
        private const string BaseRoute = Base + "/carts";
        public const string AddItemToCart = BaseRoute + "/items";
        public const string GetCart = BaseRoute;
        public const string GetCartItems = BaseRoute + "/items";
        public const string RemoveItemFromCart = BaseRoute + "/items/{itemId}";
        public const string ClearCart = BaseRoute + "/clear";
    }

    public static class Warehouses
    {
        private const string BaseRoute = Base + "/warehouse";
        public const string Get = BaseRoute;
        public const string GetAllItems = BaseRoute + "/items";
        public const string GetItemById = BaseRoute + "/items/{itemId}";
        public const string AddQuantityToItem = BaseRoute + "/items/{itemId}";
        public const string RemoveQuantityFromItem = BaseRoute + "/items/{itemId}";
    }

    public static class Orders
    {
        private const string BaseRoute = Base + "/orders";
        public const string CreateOrder = BaseRoute;
        public const string GetAllOrders = BaseRoute;
        public const string GetAllAdminOrders = BaseRoute + "/admin";
        public const string PayForOrder = BaseRoute + "/{orderId}/pay";
        public const string UpdateStatus = BaseRoute + "/{orderId}/status";
    }
}
