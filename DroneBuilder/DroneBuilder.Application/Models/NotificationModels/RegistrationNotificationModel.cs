namespace DroneBuilder.Application.Models.NotificationModels;

public class RegistrationNotificationModel : NotificationMessageModel
{
    public RegistrationNotificationModel(string userId, string userName)
    {
        Type = NotificationType.Success;
        Title = "Registration Successful";
        Message = $"Welcome to DroneBuilder, {userName}!";
        UserId = userId;
        Metadata["UserName"] = userName;
    }
}