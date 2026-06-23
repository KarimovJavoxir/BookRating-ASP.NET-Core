namespace BookRatingSystem.Api;

public static class AuthorizationPolicies
{
    public const string AdminOnly = "AdminOnly";
    public const string AuthenticatedUser = "AuthenticatedUser";
}
