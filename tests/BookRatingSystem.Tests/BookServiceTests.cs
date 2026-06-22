using BookRatingSystem.Application.Abstractions;
using BookRatingSystem.Application.Books;
using BookRatingSystem.Application.Books.Dtos;
using BookRatingSystem.Domain.Entities;
using BookRatingSystem.Domain.Exceptions;

namespace BookRatingSystem.Tests;

public class BookServiceTests
{
    [Fact]
    public async Task GetBooksAsync_calculates_average_rating_and_ratings_count()
    {
        var createdAt = new DateTimeOffset(2026, 6, 22, 10, 0, 0, TimeSpan.Zero);
        var book = Book.Create(
            Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            "Algoritmlar asoslari",
            "A. Karimov",
            "Dasturlash",
            "Algoritmlar boʻyicha oʻquv qoʻllanma.",
            2024,
            null,
            createdAt);

        book.AddRating(5, "Juda foydali", createdAt.AddMinutes(1));
        book.AddRating(3, null, createdAt.AddMinutes(2));

        var service = new BookService(
            new FakeBookRepository(book),
            new FakeUnitOfWork(),
            new FixedClock(createdAt));

        var result = await service.GetBooksAsync(CancellationToken.None);

        var item = Assert.Single(result);
        Assert.Equal(book.Id, item.Id);
        Assert.Equal(4.0m, item.AverageRating);
        Assert.Equal(2, item.RatingsCount);
    }

    [Fact]
    public async Task SubmitRatingAsync_adds_rating_and_returns_updated_details()
    {
        var now = new DateTimeOffset(2026, 6, 22, 11, 0, 0, TimeSpan.Zero);
        var book = Book.Create(
            Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            "Maʼlumotlar bazasi",
            "D. Rahimov",
            "Database",
            "Relatsion database tushunchalari.",
            2023,
            null,
            now.AddDays(-1));
        var unitOfWork = new FakeUnitOfWork();
        var service = new BookService(new FakeBookRepository(book), unitOfWork, new FixedClock(now));

        var result = await service.SubmitRatingAsync(
            book.Id,
            new SubmitBookRatingCommand(5, "Tushunarli yozilgan"),
            CancellationToken.None);

        Assert.Equal(5.0m, result.AverageRating);
        Assert.Equal(1, result.RatingsCount);
        Assert.Equal(1, unitOfWork.SaveChangesCalls);

        var rating = Assert.Single(result.RecentRatings);
        Assert.Equal(5, rating.Value);
        Assert.Equal("Tushunarli yozilgan", rating.Comment);
        Assert.Equal(now, rating.CreatedAt);
    }

    [Fact]
    public async Task SubmitRatingAsync_rejects_rating_values_outside_one_to_five()
    {
        var now = new DateTimeOffset(2026, 6, 22, 12, 0, 0, TimeSpan.Zero);
        var book = Book.Create(
            Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
            "Kompyuter tarmoqlari",
            "S. Aliyev",
            "Tarmoq",
            null,
            2022,
            null,
            now);
        var unitOfWork = new FakeUnitOfWork();
        var service = new BookService(new FakeBookRepository(book), unitOfWork, new FixedClock(now));

        await Assert.ThrowsAsync<InvalidBookRatingException>(() =>
            service.SubmitRatingAsync(
                book.Id,
                new SubmitBookRatingCommand(6, null),
                CancellationToken.None));

        Assert.Equal(0, unitOfWork.SaveChangesCalls);
    }

    private sealed class FakeBookRepository(params Book[] books) : IBookRepository
    {
        private readonly Dictionary<Guid, Book> _books = books.ToDictionary(book => book.Id);

        public Task<IReadOnlyList<Book>> ListAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<Book>>(_books.Values.ToList());
        }

        public Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            _books.TryGetValue(id, out var book);
            return Task.FromResult(book);
        }
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveChangesCalls { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveChangesCalls++;
            return Task.FromResult(1);
        }
    }

    private sealed class FixedClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
    }
}
