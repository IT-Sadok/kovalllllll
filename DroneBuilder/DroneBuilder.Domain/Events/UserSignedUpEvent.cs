namespace DroneBuilder.Domain.Events;

public class UserSignedUpEvent(Guid userId, string email) : DomainEvent
{
    public Guid UserId { get; } = userId;
    public string Email { get; init; } = email;
}