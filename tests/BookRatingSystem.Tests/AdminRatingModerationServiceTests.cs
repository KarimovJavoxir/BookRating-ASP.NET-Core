using BookRatingSystem.Application.Abstractions;
using BookRatingSystem.Application.Admin;
using BookRatingSystem.Application.Books.Dtos;
using BookRatingSystem.Domain.Entities;

namespace BookRatingSystem.Tests;

public sealed class AdminRatingModerationServiceTests
{
    [Fact]
    public async Task AcceptRatingAsync_marks_rating_verified_saves_and_reindexes_book()
    {
        var bookId = Guid.Parse("aaaaaaaa-aaaa-4aaa-aaaa-aaaaaaaaaaaa");
        var rating = BookRating.Create(
            Guid.Parse("bbbbbbbb-bbbb-4bbb-bbbb-bbbbbbbbbbbb"),
            bookId,
            Guid.Parse("cccccccc-cccc-4ccc-cccc-cccccccccccc"),
            value: 5,
            comment: "Tekshiruvdagi izoh",
            createdAt: DateTimeOffset.Parse("2026-06-26T08:00:00Z"),
            status: BookRatingStatus.NeedsHumanReview,
            banReason: "AI ishonchliligi past");
        var unitOfWork = new FakeUnitOfWork();
        var indexingService = new FakeBookIndexingService();
        var service = new AdminRatingModerationService(
            new FakeBookRatingRepository(rating),
            unitOfWork,
            indexingService);

        await service.AcceptRatingAsync(rating.Id, CancellationToken.None);

        Assert.Equal(BookRatingStatus.Verified, rating.Status);
        Assert.Null(rating.BanReason);
        Assert.Equal(1, unitOfWork.SaveChangesCalls);
        Assert.Equal([bookId], indexingService.IndexedBookIds);
    }

    [Fact]
    public async Task BanRatingAsync_marks_rating_banned_with_reason_saves_and_reindexes_book()
    {
        var bookId = Guid.Parse("dddddddd-dddd-4ddd-dddd-dddddddddddd");
        var rating = BookRating.Create(
            Guid.Parse("eeeeeeee-eeee-4eee-eeee-eeeeeeeeeeee"),
            bookId,
            Guid.Parse("ffffffff-ffff-4fff-ffff-ffffffffffff"),
            value: 1,
            comment: "Spam",
            createdAt: DateTimeOffset.Parse("2026-06-26T08:05:00Z"),
            status: BookRatingStatus.New);
        var unitOfWork = new FakeUnitOfWork();
        var indexingService = new FakeBookIndexingService();
        var service = new AdminRatingModerationService(
            new FakeBookRatingRepository(rating),
            unitOfWork,
            indexingService);

        await service.BanRatingAsync(rating.Id, "  Spam izoh  ", CancellationToken.None);

        Assert.Equal(BookRatingStatus.Banned, rating.Status);
        Assert.Equal("Spam izoh", rating.BanReason);
        Assert.Equal(1, unitOfWork.SaveChangesCalls);
        Assert.Equal([bookId], indexingService.IndexedBookIds);
    }

    [Fact]
    public async Task AcceptRatingAsync_throws_not_found_without_saving_or_indexing()
    {
        var unitOfWork = new FakeUnitOfWork();
        var indexingService = new FakeBookIndexingService();
        var service = new AdminRatingModerationService(
            new FakeBookRatingRepository(),
            unitOfWork,
            indexingService);

        await Assert.ThrowsAsync<BookRatingNotFoundException>(() =>
            service.AcceptRatingAsync(Guid.Parse("12121212-1212-4121-8121-121212121212"), CancellationToken.None));

        Assert.Equal(0, unitOfWork.SaveChangesCalls);
        Assert.Empty(indexingService.IndexedBookIds);
    }

    private sealed class FakeBookRatingRepository(params BookRating[] ratings) : IBookRatingRepository
    {
        private readonly Dictionary<Guid, BookRating> _ratings = ratings.ToDictionary(rating => rating.Id);

        public Task<BookRating?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            _ratings.TryGetValue(id, out var rating);
            return Task.FromResult(rating);
        }

        public Task<IReadOnlyList<BookRatingProcessDto>> ListPendingCommentRatingsAsync(
            int limit,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<BookRatingProcessDto>>([]);
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

    private sealed class FakeBookIndexingService : IBookIndexingService
    {
        public List<Guid> IndexedBookIds { get; } = [];

        public Task EnsureIndexAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task IndexBookAsync(Guid bookId, CancellationToken cancellationToken)
        {
            IndexedBookIds.Add(bookId);
            return Task.CompletedTask;
        }

        public Task IndexAllBooksAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task DeleteBookAsync(Guid bookId, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
