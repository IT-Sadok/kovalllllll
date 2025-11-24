namespace DroneBuilder.Application.Contexts;

public interface IUserContext
{
    public Guid UserId { get; }
    public string UserEmail { get; }
}