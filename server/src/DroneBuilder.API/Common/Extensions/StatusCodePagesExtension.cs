using DroneBuilder.API.Common.Responses;
using Microsoft.AspNetCore.WebUtilities;

namespace DroneBuilder.API.Common.Extensions;

public static class StatusCodePagesExtension
{
    public static WebApplication UseApiStatusCodePages(this WebApplication app)
    {
        app.UseStatusCodePages(async context =>
        {
            HttpResponse response = context.HttpContext.Response;

            await response.WriteAsJsonAsync(ApiResults.Failure(
                ApiResults.CodeFor(response.StatusCode),
                ReasonPhrases.GetReasonPhrase(response.StatusCode)));
        });

        return app;
    }
}
