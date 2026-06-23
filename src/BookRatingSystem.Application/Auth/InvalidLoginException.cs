namespace BookRatingSystem.Application.Auth;

public sealed class InvalidLoginException : Exception
{
    public InvalidLoginException()
        : base("Username or password is invalid.")
    {
    }
}
