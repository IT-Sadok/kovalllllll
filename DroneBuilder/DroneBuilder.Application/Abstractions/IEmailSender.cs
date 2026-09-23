using FluentResults;

namespace DroneBuilder.Application.Abstractions;

public interface IEmailSender
{
    Task<Result> SendEmailConfirmationAsync(string toEmail, Guid userId, string token,
        CancellationToken cancellationToken = default);
}
