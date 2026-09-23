using System.Text;
using DroneBuilder.Application.Common.Abstractions;
using DroneBuilder.Domain.Entities;
using FluentResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace DroneBuilder.Application.Features.Users;

public static class EmailConfirmationExtensions
{
    public static async Task<Result> SendEmailConfirmationAsync(
        this UserManager<User> userManager,
        IEmailSender emailSender,
        User user,
        CancellationToken cancellationToken)
    {
        string token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        string encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        return await emailSender.SendEmailConfirmationAsync(user.Email!, user.Id, encodedToken, cancellationToken);
    }
}
