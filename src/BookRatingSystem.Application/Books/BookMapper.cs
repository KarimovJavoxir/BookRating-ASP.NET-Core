using BookRatingSystem.Application.Books.Dtos;
using BookRatingSystem.Domain.Entities;

namespace BookRatingSystem.Application.Books;

internal static class BookMapper
{
    public static BookListItemDto ToListItem(Book book)
    {
        return new BookListItemDto(
            book.Id,
            book.Title,
            book.Author,
            book.Category,
            book.CoverImageUrl,
            CalculateAverageRating(book.Ratings),
            book.Ratings.Count,
            book.Status.ToString());
    }

    public static BookDetailsDto ToDetails(Book book)
    {
        var recentRatings = book.Ratings
            .OrderByDescending(rating => rating.CreatedAt)
            .Take(5)
            .Select(ToRatingDto)
            .ToList();

        return new BookDetailsDto(
            book.Id,
            book.Title,
            book.Author,
            book.Category,
            book.Description,
            book.PublishedYear,
            book.CoverImageUrl,
            CalculateAverageRating(book.Ratings),
            book.Ratings.Count,
            book.Status.ToString(),
            recentRatings);
    }

    private static BookRatingDto ToRatingDto(BookRating rating)
    {
        return new BookRatingDto(
            rating.Id,
            rating.BookId,
            rating.UserId,
            rating.User?.Username,
            rating.User?.ProfilePictureUrl,
            rating.Value,
            rating.Comment,
            rating.Status.ToString(),
            rating.BanReason,
            rating.CreatedAt);
    }

    private static decimal CalculateAverageRating(IReadOnlyCollection<BookRating> ratings)
    {
        if (ratings.Count == 0)
        {
            return 0m;
        }

        return Math.Round(ratings.Average(rating => (decimal)rating.Value), 2, MidpointRounding.AwayFromZero);
    }
}
