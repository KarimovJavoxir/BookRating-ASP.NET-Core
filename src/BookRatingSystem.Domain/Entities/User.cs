namespace BookRatingSystem.Domain.Entities;

public sealed class User
{
    public const int MaxUsernameLength = 64;
    public const int MaxEmailLength = 256;
    public const int MaxPasswordHashLength = 512;

    private User()
    {
    }

    private User(
        Guid id,
        string username,
        string email,
        string passwordHash,
        bool isAdmin,
        DateTimeOffset createdAt)
    {
        Id = id;
        Username = NormalizeRequired(username, nameof(username), MaxUsernameLength);
        Email = NormalizeRequired(email, nameof(email), MaxEmailLength).ToLowerInvariant();
        PasswordHash = NormalizeRequired(passwordHash, nameof(passwordHash), MaxPasswordHashLength);
        IsAdmin = isAdmin;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public bool IsAdmin { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public static User Create(
        Guid id,
        string username,
        string email,
        string passwordHash,
        bool isAdmin,
        DateTimeOffset createdAt)
    {
        return new User(id, username, email, passwordHash, isAdmin, createdAt);
    }

    private static string NormalizeRequired(string value, string parameterName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value is required.", parameterName);
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new ArgumentException($"Value cannot exceed {maxLength} characters.", parameterName);
        }

        return normalized;
    }
}
