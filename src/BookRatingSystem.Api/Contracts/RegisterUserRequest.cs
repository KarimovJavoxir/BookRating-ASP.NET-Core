using System.ComponentModel.DataAnnotations;

namespace BookRatingSystem.Api.Contracts;

public sealed class RegisterUserRequest
{
    [Required]
    [StringLength(64, MinimumLength = 3)]
    public string? Username { get; init; }

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string? Email { get; init; }

    [Required]
    [StringLength(128, MinimumLength = 8)]
    public string? Password { get; init; }
}
