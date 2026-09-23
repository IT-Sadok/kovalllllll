using FluentResults;

namespace DroneBuilder.Application.Common.Abstractions;

public interface IEmailSender
{
    Task<Result> SendEmailConfirmationAsync(string toEmail, Guid userId, string token,
        CancellationToken cancellationToken = default);
}
