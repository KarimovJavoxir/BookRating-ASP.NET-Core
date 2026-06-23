using BookRatingSystem.Application.Books.Dtos;
using BookRatingSystem.Domain.Entities;

namespace BookRatingSystem.Infrastructure.Search;

internal static class BookSearchDocumentMapper
{
    public static BookSearchDocument ToDocument(Book book)
    {
        var averageRating = CalculateAverageRating(book.Ratings);

        return new BookSearchDocument
        {
            Id = book.Id.ToString(),
            Title = book.Title,
            Author = book.Author,
            Category = book.Category,
            Description = book.Description,
            PublishedYear = book.PublishedYear,
            CoverImageUrl = book.CoverImageUrl,
            AverageRating = decimal.ToDouble(averageRating),
            RatingsCount = book.Ratings.Count,
            Status = book.Status.ToString()
        };
    }

    public static BookSearchResultDto ToSearchResult(BookSearchDocument document)
    {
        return new BookSearchResultDto(
            Guid.Parse(document.Id),
            document.Title,
            document.Author,
            document.Category,
            document.CoverImageUrl,
            Math.Round((decimal)document.AverageRating, 2, MidpointRounding.AwayFromZero),
            document.RatingsCount,
            document.Status);
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
