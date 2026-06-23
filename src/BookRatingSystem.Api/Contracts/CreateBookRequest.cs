using System.ComponentModel.DataAnnotations;
using BookRatingSystem.Domain.Entities;

namespace BookRatingSystem.Api.Contracts;

public sealed record CreateBookRequest(
    [param: Required(AllowEmptyStrings = false)]
    [param: StringLength(Book.MaxTitleLength)]
    string? Title,

    [param: Required(AllowEmptyStrings = false)]
    [param: StringLength(Book.MaxAuthorLength)]
    string? Author,

    [param: StringLength(Book.MaxCategoryLength)]
    string? Category,

    [param: StringLength(Book.MaxDescriptionLength)]
    string? Description,

    int? PublishedYear,

    [param: StringLength(Book.MaxCoverImageUrlLength)]
    string? CoverImageUrl,

    string? Status = "Verified") : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        return BookRequestValidation.Validate(Title, Author);
    }
}
