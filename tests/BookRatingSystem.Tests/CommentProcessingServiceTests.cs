using BookRatingSystem.Application.Abstractions;
using BookRatingSystem.Application.Books;
using BookRatingSystem.Application.Books.Dtos;
using BookRatingSystem.Domain.Entities;

namespace BookRatingSystem.Tests;

public sealed class CommentProcessingServiceTests
{
    [Fact]
    public async Task ProcessAsync_verifies_rating_when_ai_approves_comment()
    {
        var createdAt = new DateTimeOffset(2026, 6, 25, 9, 0, 0, TimeSpan.Zero);
        var rating = CreateRating(createdAt);
        var unitOfWork = new FakeUnitOfWork();
        var indexingService = new FakeBookIndexingService();
        var service = new CommentProcessingService(
            new FakeBookRatingRepository(rating),
            unitOfWork,
            new FakeCommentVerificationAiService(new CommentVerificationResult
            {
                ShouldBeBanned = false,
                BanExplanation = null,
                Confidence = 95
            }),
            indexingService);

        await service.ProcessAsync(new BookRatingProcessDto(rating.Id, rating.Comment!), CancellationToken.None);

        Assert.Equal(BookRatingStatus.Verified, rating.Status);
        Assert.Null(rating.BanReason);
        Assert.Equal(1, unitOfWork.SaveChangesCalls);
        Assert.Equal([rating.BookId], indexingService.IndexedBookIds);
    }

    [Fact]
    public async Task ProcessAsync_bans_rating_when_ai_rejects_comment()
    {
        var createdAt = new DateTimeOffset(2026, 6, 25, 9, 30, 0, TimeSpan.Zero);
        var rating = CreateRating(createdAt);
        var unitOfWork = new FakeUnitOfWork();
        var indexingService = new FakeBookIndexingService();
        var service = new CommentProcessingService(
            new FakeBookRatingRepository(rating),
            unitOfWork,
            new FakeCommentVerificationAiService(new CommentVerificationResult
            {
                ShouldBeBanned = true,
                BanExplanation = "Izoh kitob mavzusiga mos emas.",
                Confidence = 93
            }),
            indexingService);

        await service.ProcessAsync(new BookRatingProcessDto(rating.Id, rating.Comment!), CancellationToken.None);

        Assert.Equal(BookRatingStatus.Banned, rating.Status);
        Assert.Equal("Izoh kitob mavzusiga mos emas.", rating.BanReason);
        Assert.Equal(1, unitOfWork.SaveChangesCalls);
        Assert.Equal([rating.BookId], indexingService.IndexedBookIds);
    }

    [Fact]
    public async Task ProcessAsync_marks_rating_for_human_review_when_ai_confidence_is_low()
    {
        var createdAt = new DateTimeOffset(2026, 6, 25, 10, 0, 0, TimeSpan.Zero);
        var rating = CreateRating(createdAt);
        var unitOfWork = new FakeUnitOfWork();
        var indexingService = new FakeBookIndexingService();
        var service = new CommentProcessingService(
            new FakeBookRatingRepository(rating),
            unitOfWork,
            new FakeCommentVerificationAiService(new CommentVerificationResult
            {
                ShouldBeBanned = false,
                BanExplanation = "AI tekshiruv natijasi yetarli darajada ishonchli emas.",
                Confidence = 42
            }),
            indexingService);

        await service.ProcessAsync(new BookRatingProcessDto(rating.Id, rating.Comment!), CancellationToken.None);

        Assert.Equal(BookRatingStatus.NeedsHumanReview, rating.Status);
        Assert.Equal("AI tekshiruv natijasi yetarli darajada ishonchli emas.", rating.BanReason);
        Assert.Equal(1, unitOfWork.SaveChangesCalls);
        Assert.Equal([rating.BookId], indexingService.IndexedBookIds);
    }

    [Fact]
    public async Task ProcessAsync_ignores_rating_that_was_already_processed()
    {
        var createdAt = new DateTimeOffset(2026, 6, 25, 10, 30, 0, TimeSpan.Zero);
        var rating = BookRating.Create(
            Guid.Parse("33333333-3333-4333-8333-333333333333"),
            Guid.Parse("44444444-4444-4444-8444-444444444444"),
            Guid.Parse("55555555-5555-4555-8555-555555555555"),
            value: 4,
            comment: "Oldin tekshirilgan izoh.",
            createdAt,
            BookRatingStatus.Verified);
        var unitOfWork = new FakeUnitOfWork();
        var indexingService = new FakeBookIndexingService();
        var aiService = new FakeCommentVerificationAiService(new CommentVerificationResult
        {
            ShouldBeBanned = true,
            BanExplanation = "Bu natija ishlatilmasligi kerak.",
            Confidence = 99
        });
        var service = new CommentProcessingService(
            new FakeBookRatingRepository(rating),
            unitOfWork,
            aiService,
            indexingService);

        await service.ProcessAsync(new BookRatingProcessDto(rating.Id, rating.Comment!), CancellationToken.None);

        Assert.Equal(BookRatingStatus.Verified, rating.Status);
        Assert.Equal(0, aiService.VerifyCalls);
        Assert.Equal(0, unitOfWork.SaveChangesCalls);
        Assert.Empty(indexingService.IndexedBookIds);
    }

    private static BookRating CreateRating(DateTimeOffset createdAt)
    {
        return BookRating.Create(
            Guid.Parse("11111111-1111-4111-8111-111111111111"),
            Guid.Parse("22222222-2222-4222-8222-222222222222"),
            Guid.Parse("55555555-5555-4555-8555-555555555555"),
            value: 5,
            comment: "Kitob mazmuni tushunarli bayon qilingan.",
            createdAt);
    }

    private sealed class FakeBookRatingRepository(BookRating? rating) : IBookRatingRepository
    {
        public Task<BookRating?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(rating?.Id == id ? rating : null);
        }

        public Task<IReadOnlyList<BookRatingProcessDto>> ListPendingCommentRatingsAsync(
            int limit,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<BookRatingProcessDto>>([]);
        }
    }

    private sealed class FakeCommentVerificationAiService(CommentVerificationResult result)
        : ICommentVerificationAiService
    {
        public int VerifyCalls { get; private set; }

        public Task<CommentVerificationResult> VerifyAsync(
            string comment,
            CancellationToken cancellationToken = default)
        {
            VerifyCalls++;
            return Task.FromResult(result);
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
