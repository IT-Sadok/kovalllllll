namespace DroneBuilder.API.Common.Routes;

public abstract class ApiRoutes
{
    public const string Base = "/api";

    public static class Users
    {
        private const string BaseRoute = Base + "/users";
        public const string SignUp = BaseRoute + "/sign-up";
        public const string SignIn = BaseRoute + "/sign-in";
        public const string ConfirmEmail = BaseRoute + "/confirm-email";
        public const string ResendConfirmation = BaseRoute + "/resend-confirmation";
        public const string SignOut = BaseRoute + "/sign-out";
        public const string Me = BaseRoute + "/me";
    }

    public static class Products
    {
        private const string BaseRoute = Base + "/products";
        public const string GetAll = BaseRoute;
        public const string GetById = BaseRoute + "/{productId}";
        public const string Create = BaseRoute;
        public const string Update = BaseRoute + "/{productId}";
        public const string Delete = BaseRoute + "/{productId}";
        public const string GetDelisted = BaseRoute + "/delisted";
        public const string GetManufacturers = BaseRoute + "/manufacturers";
        public const string Restore = BaseRoute + "/{productId}/restore";
        public const string SetSpec = BaseRoute + "/{productId}/spec";
        public const string RemoveSpec = BaseRoute + "/{productId}/spec";
        public const string SetAttributes = BaseRoute + "/{productId}/attributes";
    }

    public static class Builds
    {
        private const string BaseRoute = Base + "/builds";
        public const string Check = BaseRoute + "/check";
        public const string GetAll = BaseRoute;
        public const string GetById = BaseRoute + "/{buildId:guid}";
        public const string Create = BaseRoute;
        public const string Update = BaseRoute + "/{buildId:guid}";
        public const string Delete = BaseRoute + "/{buildId:guid}";
    }

    public static class Imports
    {
        private const string BaseRoute = Base + "/imports";
        public const string GetAll = BaseRoute;
        public const string GetById = BaseRoute + "/{importRunId}";
        public const string StartRaceDayQuads = BaseRoute + "/racedayquads";
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
        public const string AddItemsToCart = BaseRoute + "/items/batch";
        public const string GetCart = BaseRoute;
        public const string GetCartItems = BaseRoute + "/items";
        public const string UpdateItemQuantity = BaseRoute + "/items/{productId}";
        public const string RemoveItemFromCart = BaseRoute + "/items/{productId}";
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

    public static class Payments
    {
        private const string BaseRoute = Base + "/payments";
        public const string StripeWebhook = BaseRoute + "/stripe/webhook";
    }

    public static class Orders
    {
        private const string BaseRoute = Base + "/orders";
        public const string CreateOrder = BaseRoute;
        public const string GetAllOrders = BaseRoute;
        public const string GetAllAdminOrders = BaseRoute + "/admin";
        public const string StartPayment = BaseRoute + "/{orderId}/payment";
        public const string CancelOrder = BaseRoute + "/{orderId}/cancel";
        public const string UpdateStatus = BaseRoute + "/{orderId}/status";
    }
}
