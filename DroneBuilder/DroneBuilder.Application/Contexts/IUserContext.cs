namespace DroneBuilder.Application.Contexts;

public interface IUserContext
{
    Guid UserId { get; }
    string UserEmail { get; }
}
