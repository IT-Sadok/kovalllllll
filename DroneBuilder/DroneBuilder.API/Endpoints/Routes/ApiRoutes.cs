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
        public const string GetPropertiesByProductId = BaseRoute + "/{productId}/properties";
        public const string AssignPropertyToProduct = BaseRoute + "/{productId}/properties/{propertyId}";
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

    public static class Images
    {
        private const string BaseRoute = Base + "/images";
        public const string Upload = BaseRoute + "/upload";
        public const string Delete = BaseRoute + "/{imageId}";
        public const string GetById = BaseRoute + "/{imageId}";
        public const string GetAll = BaseRoute;
        public const string GetImagesByProductId = BaseRoute + "/product/{productId}";
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
        public const string UpdateWarehouseItem = BaseRoute + "/items/{itemId}";
        public const string GetItemById = BaseRoute + "/items/{itemId}";
        public const string AddQuantityToItem = BaseRoute + "/items/{itemId}";
        public const string RemoveQuantityFromItem = BaseRoute + "/items/{itemId}";
    }

    public static class Orders
    {
        private const string BaseRoute = Base + "/orders";
        public const string CreateOrder = BaseRoute;
        public const string GetOrderById = BaseRoute + "/{orderId}";
        public const string GetAllOrders = BaseRoute;
        public const string PayForOrder = BaseRoute + "/{orderId}/pay";
    }
}