using System.ComponentModel.DataAnnotations;

namespace BookRatingSystem.Api.Contracts;

public sealed class LoginRequest
{
    [Required]
    public string? Username { get; init; }

    [Required]
    public string? Password { get; init; }
}
