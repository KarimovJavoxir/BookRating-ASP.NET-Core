using BookRatingSystem.Application.Abstractions;
using BookRatingSystem.Application.Books;
using BookRatingSystem.Application.Books.Dtos;
using BookRatingSystem.Application.Common;
using BookRatingSystem.Domain.Entities;
using BookRatingSystem.Domain.Exceptions;
using System.Reflection;

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

        book.AddRating(Guid.Parse("10000000-0000-0000-0000-000000000001"), 5, "Juda foydali", createdAt.AddMinutes(1));
        book.AddRating(Guid.Parse("10000000-0000-0000-0000-000000000002"), 3, null, createdAt.AddMinutes(2));

        var service = new BookService(
            new FakeBookRepository(book),
            new FakeUnitOfWork(),
            new FixedClock(createdAt),
            new FakeBookIndexingService());

        var result = await service.GetBooksAsync(new PaginationQuery(), CancellationToken.None);

        var item = Assert.Single(result.Items);
        Assert.Equal(book.Id, item.Id);
        Assert.Equal(4.0m, item.AverageRating);
        Assert.Equal(2, item.RatingsCount);
    }

    [Fact]
    public async Task GetBooksAsync_returns_only_verified_books_for_public_catalog()
    {
        var createdAt = new DateTimeOffset(2026, 6, 23, 16, 0, 0, TimeSpan.Zero);
        var verifiedBook = Book.Create(
            Guid.Parse("10101010-1010-1010-1010-101010101010"),
            "Tasdiqlangan kitob",
            "A. Muallif",
            "Dasturlash",
            null,
            2026,
            null,
            createdAt,
            status: BookStatus.Verified);
        var unverifiedBook = Book.Create(
            Guid.Parse("20202020-2020-2020-2020-202020202020"),
            "Yangi kitob",
            "B. Muallif",
            "Dasturlash",
            null,
            2026,
            null,
            createdAt,
            status: BookStatus.New);
        var bannedBook = Book.Create(
            Guid.Parse("21212121-2121-2121-2121-212121212121"),
            "Bloklangan kitob",
            "C. Muallif",
            "Dasturlash",
            null,
            2026,
            null,
            createdAt,
            status: BookStatus.Banned);
        var service = new BookService(
            new FakeBookRepository(verifiedBook, unverifiedBook, bannedBook),
            new FakeUnitOfWork(),
            new FixedClock(createdAt),
            new FakeBookIndexingService());

        var result = await service.GetBooksAsync(new PaginationQuery(), CancellationToken.None);

        var item = Assert.Single(result.Items);
        Assert.Equal(verifiedBook.Id, item.Id);
        Assert.Equal(BookStatus.Verified.ToString(), item.Status);
    }

    [Fact]
    public async Task GetBooksAsync_returns_requested_page_with_total_metadata()
    {
        var createdAt = new DateTimeOffset(2026, 6, 24, 9, 0, 0, TimeSpan.Zero);
        var books = Enumerable.Range(1, 5)
            .Select(index => Book.Create(
                Guid.Parse($"aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa{index}"),
                $"Kitob {index}",
                "A. Muallif",
                "Dasturlash",
                null,
                2026,
                null,
                createdAt,
                status: BookStatus.Verified))
            .ToArray();
        var service = new BookService(
            new FakeBookRepository(books),
            new FakeUnitOfWork(),
            new FixedClock(createdAt),
            new FakeBookIndexingService());

        var result = await service.GetBooksAsync(new PaginationQuery(page: 2, pageSize: 2), CancellationToken.None);

        Assert.Equal(2, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
        Assert.Equal(["Kitob 3", "Kitob 4"], result.Items.Select(book => book.Title));
    }

    [Fact]
    public async Task GetBookByIdAsync_rejects_unverified_books_for_public_details()
    {
        var createdAt = new DateTimeOffset(2026, 6, 23, 16, 30, 0, TimeSpan.Zero);
        var unverifiedBook = Book.Create(
            Guid.Parse("30303030-3030-3030-3030-303030303030"),
            "Yashirilgan kitob",
            "C. Muallif",
            "Database",
            null,
            2026,
            null,
            createdAt,
            status: BookStatus.New);
        var service = new BookService(
            new FakeBookRepository(unverifiedBook),
            new FakeUnitOfWork(),
            new FixedClock(createdAt),
            new FakeBookIndexingService());

        await Assert.ThrowsAsync<BookNotFoundException>(() =>
            service.GetBookByIdAsync(unverifiedBook.Id, CancellationToken.None));
    }

    [Fact]
    public async Task SubmitRatingAsync_adds_rating_and_returns_updated_details()
    {
        var now = new DateTimeOffset(2026, 6, 22, 11, 0, 0, TimeSpan.Zero);
        var userId = Guid.Parse("10000000-0000-0000-0000-000000000001");
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
        var indexingService = new FakeBookIndexingService();
        var service = new BookService(
            new FakeBookRepository(book),
            unitOfWork,
            new FixedClock(now),
            indexingService);

        var result = await service.SubmitRatingAsync(
            book.Id,
            new SubmitBookRatingCommand(userId, 5, "Tushunarli yozilgan"),
            CancellationToken.None);

        Assert.Equal(5.0m, result.AverageRating);
        Assert.Equal(1, result.RatingsCount);
        Assert.Equal(1, unitOfWork.SaveChangesCalls);
        Assert.Equal([book.Id], indexingService.IndexedBookIds);

        var rating = Assert.Single(result.RecentRatings);
        Assert.Equal(userId, rating.UserId);
        Assert.Equal(5, rating.Value);
        Assert.Equal("Tushunarli yozilgan", rating.Comment);
        Assert.Equal(BookRatingStatus.New.ToString(), rating.Status);
        Assert.Null(rating.BanReason);
        Assert.Equal(now, rating.CreatedAt);
    }

    [Fact]
    public async Task GetBookByIdAsync_includes_rating_status_and_ban_reason()
    {
        var now = new DateTimeOffset(2026, 6, 23, 15, 30, 0, TimeSpan.Zero);
        var userId = Guid.Parse("10000000-0000-0000-0000-000000000012");
        var book = Book.Create(
            Guid.Parse("98989898-9898-9898-9898-989898989898"),
            "Rating status testi",
            "A. Muallif",
            "Test",
            null,
            2026,
            null,
            now);
        book.Ratings.Add(BookRating.Create(
            Guid.Parse("77777777-7777-7777-7777-777777777777"),
            book.Id,
            userId,
            2,
            "Mos emas",
            now.AddMinutes(1),
            BookRatingStatus.Banned,
            "  Notoʻgʻri izoh  "));

        var service = new BookService(
            new FakeBookRepository(book),
            new FakeUnitOfWork(),
            new FixedClock(now),
            new FakeBookIndexingService());

        var result = await service.GetBookByIdAsync(book.Id, CancellationToken.None);

        var recentRating = Assert.Single(result.RecentRatings);
        Assert.Equal(BookRatingStatus.Banned.ToString(), recentRating.Status);
        Assert.Equal("Notoʻgʻri izoh", recentRating.BanReason);
    }

    [Fact]
    public async Task GetBookByIdAsync_includes_recent_rating_user_profile_data()
    {
        var now = new DateTimeOffset(2026, 6, 23, 15, 0, 0, TimeSpan.Zero);
        var userId = Guid.Parse("10000000-0000-0000-0000-000000000011");
        var book = Book.Create(
            Guid.Parse("99999999-9999-9999-9999-999999999999"),
            "Profil rasmi testi",
            "A. Muallif",
            "Test",
            null,
            2026,
            null,
            now);
        var rating = book.AddRating(userId, 4, "Yaxshi", now.AddMinutes(1));
        var user = User.Create(
            userId,
            "user11",
            "user11@example.com",
            "hashed-password",
            "https://example.com/users/user11.jpg",
            isAdmin: false,
            createdAt: now.AddDays(-1));
        SetRatingUser(rating, user);

        var service = new BookService(
            new FakeBookRepository(book),
            new FakeUnitOfWork(),
            new FixedClock(now),
            new FakeBookIndexingService());

        var result = await service.GetBookByIdAsync(book.Id, CancellationToken.None);

        var recentRating = Assert.Single(result.RecentRatings);
        Assert.Equal("user11", recentRating.Username);
        Assert.Equal("https://example.com/users/user11.jpg", recentRating.UserProfilePictureUrl);
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
        var indexingService = new FakeBookIndexingService();
        var service = new BookService(
            new FakeBookRepository(book),
            unitOfWork,
            new FixedClock(now),
            indexingService);

        await Assert.ThrowsAsync<InvalidBookRatingException>(() =>
            service.SubmitRatingAsync(
                book.Id,
                new SubmitBookRatingCommand(
                    Guid.Parse("10000000-0000-0000-0000-000000000002"),
                    6,
                    null),
                CancellationToken.None));

        Assert.Equal(0, unitOfWork.SaveChangesCalls);
        Assert.Empty(indexingService.IndexedBookIds);
    }

    [Fact]
    public async Task CreateBookAsync_saves_book_and_indexes_it()
    {
        var now = new DateTimeOffset(2026, 6, 23, 9, 0, 0, TimeSpan.Zero);
        var repository = new FakeBookRepository();
        var unitOfWork = new FakeUnitOfWork();
        var indexingService = new FakeBookIndexingService();
        var service = new BookService(
            repository,
            unitOfWork,
            new FixedClock(now),
            indexingService);

        var result = await service.CreateBookAsync(
            new CreateBookCommand(
                "  Clean Code  ",
                "  Robert C. Martin ",
                "Dasturlash",
                "Kod sifatini oshirish haqida kitob.",
                2008,
                "https://example.com/clean-code.jpg"),
            CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Clean Code", result.Title);
        Assert.Equal("Robert C. Martin", result.Author);
        Assert.Equal("Dasturlash", result.Category);
        Assert.Equal("Kod sifatini oshirish haqida kitob.", result.Description);
        Assert.Equal(2008, result.PublishedYear);
        Assert.Equal("https://example.com/clean-code.jpg", result.CoverImageUrl);
        Assert.Equal(0m, result.AverageRating);
        Assert.Equal(0, result.RatingsCount);
        Assert.Equal(1, unitOfWork.SaveChangesCalls);
        Assert.Equal([result.Id], indexingService.IndexedBookIds);

        var persistedBook = await repository.GetByIdAsync(result.Id, CancellationToken.None);
        Assert.NotNull(persistedBook);
        Assert.Equal(now, persistedBook.CreatedAt);
        Assert.Equal(now, persistedBook.UpdatedAt);
    }

    [Fact]
    public async Task UpdateBookAsync_updates_book_and_reindexes_with_existing_rating_statistics()
    {
        var createdAt = new DateTimeOffset(2026, 6, 20, 9, 0, 0, TimeSpan.Zero);
        var now = new DateTimeOffset(2026, 6, 23, 10, 0, 0, TimeSpan.Zero);
        var book = Book.Create(
            Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
            "Eski nom",
            "Eski muallif",
            "Eski kategoriya",
            "Eski izoh",
            2020,
            null,
            createdAt);
        book.AddRating(Guid.Parse("10000000-0000-0000-0000-000000000003"), 5, null, createdAt.AddMinutes(1));
        book.AddRating(Guid.Parse("10000000-0000-0000-0000-000000000004"), 3, null, createdAt.AddMinutes(2));

        var unitOfWork = new FakeUnitOfWork();
        var indexingService = new FakeBookIndexingService();
        var service = new BookService(
            new FakeBookRepository(book),
            unitOfWork,
            new FixedClock(now),
            indexingService);

        var result = await service.UpdateBookAsync(
            book.Id,
            new UpdateBookCommand(
                "Yangi nom",
                "Yangi muallif",
                "Yangi kategoriya",
                "Yangi izoh",
                2025,
                "https://example.com/new.jpg"),
            CancellationToken.None);

        Assert.Equal(book.Id, result.Id);
        Assert.Equal("Yangi nom", result.Title);
        Assert.Equal("Yangi muallif", result.Author);
        Assert.Equal("Yangi kategoriya", result.Category);
        Assert.Equal("Yangi izoh", result.Description);
        Assert.Equal(2025, result.PublishedYear);
        Assert.Equal("https://example.com/new.jpg", result.CoverImageUrl);
        Assert.Equal(4.0m, result.AverageRating);
        Assert.Equal(2, result.RatingsCount);
        Assert.Equal(now, book.UpdatedAt);
        Assert.Equal(1, unitOfWork.SaveChangesCalls);
        Assert.Equal([book.Id], indexingService.IndexedBookIds);
    }

    [Fact]
    public async Task DeleteBookAsync_removes_book_and_deletes_index_document()
    {
        var now = new DateTimeOffset(2026, 6, 23, 11, 0, 0, TimeSpan.Zero);
        var book = Book.Create(
            Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
            "Oʻchiriladigan kitob",
            "A. Muallif",
            null,
            null,
            null,
            null,
            now.AddDays(-1));
        var repository = new FakeBookRepository(book);
        var unitOfWork = new FakeUnitOfWork();
        var indexingService = new FakeBookIndexingService();
        var service = new BookService(
            repository,
            unitOfWork,
            new FixedClock(now),
            indexingService);

        await service.DeleteBookAsync(book.Id, CancellationToken.None);

        Assert.Null(await repository.GetByIdAsync(book.Id, CancellationToken.None));
        Assert.Equal(1, unitOfWork.SaveChangesCalls);
        Assert.Equal([book.Id], indexingService.DeletedBookIds);
    }

    [Fact]
    public async Task UpdateBookAsync_throws_not_found_without_saving_or_indexing()
    {
        var now = new DateTimeOffset(2026, 6, 23, 12, 0, 0, TimeSpan.Zero);
        var missingBookId = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff");
        var unitOfWork = new FakeUnitOfWork();
        var indexingService = new FakeBookIndexingService();
        var service = new BookService(
            new FakeBookRepository(),
            unitOfWork,
            new FixedClock(now),
            indexingService);

        var exception = await Assert.ThrowsAsync<BookNotFoundException>(() =>
            service.UpdateBookAsync(
                missingBookId,
                new UpdateBookCommand("Nom", "Muallif", null, null, null, null),
                CancellationToken.None));

        Assert.Equal(missingBookId, exception.BookId);
        Assert.Equal(0, unitOfWork.SaveChangesCalls);
        Assert.Empty(indexingService.IndexedBookIds);
    }

    [Fact]
    public async Task DeleteBookAsync_throws_not_found_without_saving_or_deleting_index_document()
    {
        var now = new DateTimeOffset(2026, 6, 23, 13, 0, 0, TimeSpan.Zero);
        var missingBookId = Guid.Parse("abababab-abab-abab-abab-abababababab");
        var unitOfWork = new FakeUnitOfWork();
        var indexingService = new FakeBookIndexingService();
        var service = new BookService(
            new FakeBookRepository(),
            unitOfWork,
            new FixedClock(now),
            indexingService);

        var exception = await Assert.ThrowsAsync<BookNotFoundException>(() =>
            service.DeleteBookAsync(missingBookId, CancellationToken.None));

        Assert.Equal(missingBookId, exception.BookId);
        Assert.Equal(0, unitOfWork.SaveChangesCalls);
        Assert.Empty(indexingService.DeletedBookIds);
    }

    [Fact]
    public async Task CreateBookAsync_keeps_saved_book_when_indexing_fails()
    {
        var now = new DateTimeOffset(2026, 6, 23, 14, 0, 0, TimeSpan.Zero);
        var repository = new FakeBookRepository();
        var unitOfWork = new FakeUnitOfWork();
        var indexingService = new FakeBookIndexingService { ThrowOnIndex = true };
        var service = new BookService(
            repository,
            unitOfWork,
            new FixedClock(now),
            indexingService);

        var result = await service.CreateBookAsync(
            new CreateBookCommand("Index xato testi", "A. Muallif", null, null, null, null),
            CancellationToken.None);

        Assert.Equal(1, unitOfWork.SaveChangesCalls);
        Assert.NotNull(await repository.GetByIdAsync(result.Id, CancellationToken.None));
        Assert.Equal([result.Id], indexingService.IndexedBookIds);
    }

    private sealed class FakeBookRepository(params Book[] books) : IBookRepository
    {
        private readonly Dictionary<Guid, Book> _books = books.ToDictionary(book => book.Id);

        public Task<PagedResult<Book>> ListAsync(PaginationQuery pagination, CancellationToken cancellationToken)
        {
            return Task.FromResult(ToPagedResult(_books.Values.OrderBy(book => book.Title), pagination));
        }

        public Task<PagedResult<Book>> ListVerifiedAsync(PaginationQuery pagination, CancellationToken cancellationToken)
        {
            return Task.FromResult(ToPagedResult(
                _books.Values
                    .Where(book => book.Status == BookStatus.Verified)
                    .OrderBy(book => book.Title),
                pagination));
        }

        public Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            _books.TryGetValue(id, out var book);
            return Task.FromResult(book);
        }

        public void Add(Book book)
        {
            _books.Add(book.Id, book);
        }

        public void Delete(Book book)
        {
            _books.Remove(book.Id);
        }

        private static PagedResult<Book> ToPagedResult(IEnumerable<Book> books, PaginationQuery pagination)
        {
            var items = books.ToList();
            return new PagedResult<Book>(
                items
                    .Skip(pagination.Skip)
                    .Take(pagination.PageSize)
                    .ToList(),
                pagination.Page,
                pagination.PageSize,
                items.Count);
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

    private sealed class FakeBookIndexingService : IBookIndexingService
    {
        public List<Guid> IndexedBookIds { get; } = [];

        public List<Guid> DeletedBookIds { get; } = [];

        public bool ThrowOnIndex { get; init; }

        public Task EnsureIndexAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task IndexBookAsync(Guid bookId, CancellationToken cancellationToken)
        {
            IndexedBookIds.Add(bookId);
            if (ThrowOnIndex)
            {
                throw new InvalidOperationException("Index sync failed.");
            }

            return Task.CompletedTask;
        }

        public Task IndexAllBooksAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task DeleteBookAsync(Guid bookId, CancellationToken cancellationToken)
        {
            DeletedBookIds.Add(bookId);
            return Task.CompletedTask;
        }
    }

    private static void SetRatingUser(BookRating rating, User user)
    {
        typeof(BookRating)
            .GetProperty(nameof(BookRating.User), BindingFlags.Instance | BindingFlags.Public)!
            .SetValue(rating, user);
    }
}
