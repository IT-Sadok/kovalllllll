using FluentResults;

namespace DroneBuilder.Application.Abstractions;

public interface IEmailSender
{
    /// <summary>
    /// Sends the account confirmation email. The implementation owns the link format,
    /// so the application layer does not need to know the public URL of the API.
    /// </summary>
    Task<Result> SendEmailConfirmationAsync(string toEmail, Guid userId, string token,
        CancellationToken cancellationToken = default);
}
