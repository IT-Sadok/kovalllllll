namespace DroneBuilder.Domain.Events.UserEvents;

public class UserSignedInEvent(Guid userId, string email) : DomainEvent
{
    public Guid UserId { get; } = userId;
    public string Email { get; init; } = email;
}