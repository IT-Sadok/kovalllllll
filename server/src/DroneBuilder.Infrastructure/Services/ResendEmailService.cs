using System.Net.Http.Json;
using System.Text.Json.Serialization;
using DroneBuilder.Application.Common.Abstractions;
using DroneBuilder.Application.Common.Errors;
using DroneBuilder.Infrastructure.Options;
using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DroneBuilder.Infrastructure.Services;

public class ResendEmailService(
    HttpClient httpClient,
    IOptions<ResendOptions> options,
    ILogger<ResendEmailService> logger) : IEmailSender
{
    private readonly ResendOptions _options = options.Value;

    public async Task<Result> SendEmailConfirmationAsync(string toEmail, Guid userId, string token,
        CancellationToken cancellationToken = default)
    {
        string confirmationLink =
            $"{_options.ConfirmationUrl}?userId={userId}&token={Uri.EscapeDataString(token)}";

        var request = new ResendEmailRequest(
            From: _options.FromEmail,
            To: [toEmail],
            Subject: "Confirm your DroneBuilder account",
            Html: $"""
                   <p>Welcome to DroneBuilder!</p>
                   <p>Please confirm your account by following this link:</p>
                   <p><a href="{confirmationLink}">Confirm my account</a></p>
                   """);

        try
        {
            HttpResponseMessage response = await httpClient.PostAsJsonAsync("emails", request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("Confirmation email sent to {Email}.", toEmail);
                return Result.Ok();
            }

            string body = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogError("Resend rejected the confirmation email for {Email}. Status: {Status}. Body: {Body}",
                toEmail, (int)response.StatusCode, body);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending confirmation email to {Email}.", toEmail);
        }

        return Result.Fail(new BadRequestError("Could not send the confirmation email. Please try again later."));
    }

    private sealed record ResendEmailRequest(
        [property: JsonPropertyName("from")] string From,
        [property: JsonPropertyName("to")] string[] To,
        [property: JsonPropertyName("subject")] string Subject,
        [property: JsonPropertyName("html")] string Html);
}
