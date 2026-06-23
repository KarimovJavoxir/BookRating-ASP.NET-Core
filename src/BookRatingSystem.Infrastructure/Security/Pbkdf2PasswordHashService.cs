using System.Security.Cryptography;
using BookRatingSystem.Application.Abstractions;

namespace BookRatingSystem.Infrastructure.Security;

internal sealed class Pbkdf2PasswordHashService : IPasswordHashService
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100_000;
    private const string Prefix = "pbkdf2-sha256";

    public string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        return HashPassword(password, salt);
    }

    public bool VerifyPassword(string passwordHash, string password)
    {
        var parts = passwordHash.Split(':');
        if (parts.Length != 4 || parts[0] != Prefix || !int.TryParse(parts[1], out var iterations))
        {
            return false;
        }

        var salt = Convert.FromBase64String(parts[2]);
        var expectedHash = Convert.FromBase64String(parts[3]);
        var actualHash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            expectedHash.Length);

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }

    internal static string CreateSeedHash(string password, string saltText)
    {
        var salt = System.Text.Encoding.UTF8.GetBytes(saltText);
        return HashPassword(password, salt);
    }

    private static string HashPassword(string password, byte[] salt)
    {
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            HashSize);

        return $"{Prefix}:{Iterations}:{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }
}
