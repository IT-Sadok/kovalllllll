namespace DroneBuilder.Application.Common.Contexts;

public interface IUserContext
{
    Guid UserId { get; }
    string UserEmail { get; }
}
