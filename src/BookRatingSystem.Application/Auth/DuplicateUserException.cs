namespace BookRatingSystem.Application.Auth;

public sealed class DuplicateUserException : Exception
{
    public DuplicateUserException()
        : base("Username or email already exists.")
    {
    }
}
