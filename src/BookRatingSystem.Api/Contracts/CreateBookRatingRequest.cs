using System.ComponentModel.DataAnnotations;
using BookRatingSystem.Domain.Entities;

namespace BookRatingSystem.Api.Contracts;

public sealed record CreateBookRatingRequest(
    [param: Range(BookRating.MinValue, BookRating.MaxValue)]
    int Value,

    [param: StringLength(BookRating.MaxCommentLength)]
    string? Comment);
