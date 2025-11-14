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
    }
}