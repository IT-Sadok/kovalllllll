namespace DroneBuilder.API.Extensions;

public static class SpaExtension
{
    private static bool HasSpaAssets(IWebHostEnvironment env)
    {
        string webRootSegment = "wwwroot".TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        string webRootPath = Path.Join(env.ContentRootPath, webRootSegment);
        return Directory.Exists(webRootPath) && File.Exists(Path.Combine(webRootPath, "index.html"));
    }

    public static WebApplication UseSpaStaticFiles(this WebApplication app)
    {
        if (HasSpaAssets(app.Environment))
        {
            app.UseDefaultFiles();
            app.UseStaticFiles();
        }

        return app;
    }

    public static WebApplication MapSpaFallback(this WebApplication app)
    {
        if (HasSpaAssets(app.Environment))
        {
            app.MapFallbackToFile("index.html");
        }

        return app;
    }
}
