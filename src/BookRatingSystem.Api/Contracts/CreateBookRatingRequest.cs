using System.ComponentModel.DataAnnotations;
using BookRatingSystem.Domain.Entities;

namespace BookRatingSystem.Api.Contracts;

public sealed record CreateBookRatingRequest(
    [property: Range(BookRating.MinValue, BookRating.MaxValue)]
    int Value,

    [property: StringLength(BookRating.MaxCommentLength)]
    string? Comment);
