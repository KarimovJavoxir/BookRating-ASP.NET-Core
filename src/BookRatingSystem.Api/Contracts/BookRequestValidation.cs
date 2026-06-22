using System.ComponentModel.DataAnnotations;

namespace BookRatingSystem.Api.Contracts;

internal static class BookRequestValidation
{
    public static IEnumerable<ValidationResult> Validate(string? title, string? author)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            yield return new ValidationResult("Title is required.", [nameof(CreateBookRequest.Title)]);
        }

        if (string.IsNullOrWhiteSpace(author))
        {
            yield return new ValidationResult("Author is required.", [nameof(CreateBookRequest.Author)]);
        }
    }
}
