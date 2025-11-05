namespace DroneBuilder.Application.Exceptions;

public class InvalidEmailOrPasswordException : Exception
{
    public InvalidEmailOrPasswordException()
    {
    }

    public InvalidEmailOrPasswordException(string message)
        : base(message)
    {
    }

    public InvalidEmailOrPasswordException(string message, Exception inner)
        : base(message, inner)
    {
    }
}