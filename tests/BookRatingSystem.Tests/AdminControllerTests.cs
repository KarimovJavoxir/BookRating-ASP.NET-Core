using BookRatingSystem.Api.Controllers;
using BookRatingSystem.Api.Contracts;
using BookRatingSystem.Application.Admin;
using BookRatingSystem.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace BookRatingSystem.Tests;

public sealed class AdminControllerTests
{
    [Fact]
    public async Task GetDashboard_returns_date_filtered_dashboard()
    {
        var from = new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2026, 6, 23, 23, 59, 59, TimeSpan.Zero);
        var dashboard = new AdminDashboardDto(
            TotalBooks: 10,
            TotalUsers: 21,
            TotalRatings: 45,
            BooksAddedInRange: 3,
            RatingsAddedInRange: 8,
            AverageRatingInRange: 4.25m,
            RecentRatings: []);
        var service = new FakeAdminReadService(dashboard);
        var controller = new AdminController(service, new FakeAdminRatingModerationService());

        var response = await controller.GetDashboard(from, to, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        Assert.Same(dashboard, okResult.Value);
        Assert.Equal(from, service.LastFrom);
        Assert.Equal(to, service.LastTo);
    }

    [Fact]
    public async Task GetBooks_returns_requested_admin_books_page_including_unverified()
    {
        var books = new PagedResult<AdminBookDto>(
        [
            new AdminBookDto(
                Guid.Parse("40404040-4040-4040-4040-404040404040"),
                "Tasdiqlanmagan kitob",
                "A. Muallif",
                "Dasturlash",
                null,
                2026,
                null,
                Status: "New",
                AverageRating: 0,
                RatingsCount: 0),
        ], Page: 2, PageSize: 10, TotalCount: 11);
        var service = new FakeAdminReadService(
            new AdminDashboardDto(1, 0, 0, 1, 0, 0, []),
            books: books);
        var controller = new AdminController(service, new FakeAdminRatingModerationService());

        var response = await controller.GetBooks(page: 2, pageSize: 10, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        Assert.Same(books, okResult.Value);
        Assert.Equal(2, service.LastBooksPagination?.Page);
        Assert.Equal(10, service.LastBooksPagination?.PageSize);
    }

    [Fact]
    public async Task GetUsers_returns_requested_users_page()
    {
        var users = new PagedResult<AdminUserDto>(
        [
            new AdminUserDto(
                Guid.Parse("50505050-5050-5050-5050-505050505050"),
                "user01",
                "user01@example.com",
                null,
                IsAdmin: false,
                CreatedAt: DateTimeOffset.Parse("2026-06-24T09:00:00Z"),
                RatingsCount: 4),
        ], Page: 3, PageSize: 5, TotalCount: 13);
        var service = new FakeAdminReadService(new AdminDashboardDto(0, 1, 0, 0, 0, 0, []), users: users);
        var controller = new AdminController(service, new FakeAdminRatingModerationService());

        var response = await controller.GetUsers(page: 3, pageSize: 5, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        Assert.Same(users, okResult.Value);
        Assert.Equal(3, service.LastUsersPagination?.Page);
        Assert.Equal(5, service.LastUsersPagination?.PageSize);
    }

    [Fact]
    public async Task GetRatings_returns_requested_ratings_page()
    {
        var ratings = new PagedResult<AdminBookRatingDto>(
        [
            new AdminBookRatingDto(
                Guid.Parse("60606060-6060-6060-6060-606060606060"),
                Guid.Parse("70707070-7070-7070-7070-707070707070"),
                "Algoritmlar",
                Guid.Parse("80808080-8080-8080-8080-808080808080"),
                "user01",
                null,
                Value: 5,
                Comment: "Foydali",
                Status: "New",
                BanReason: null,
                CreatedAt: DateTimeOffset.Parse("2026-06-24T10:00:00Z")),
        ], Page: 4, PageSize: 20, TotalCount: 63);
        var service = new FakeAdminReadService(new AdminDashboardDto(0, 0, 1, 0, 1, 5, []), ratings: ratings);
        var controller = new AdminController(service, new FakeAdminRatingModerationService());

        var response = await controller.GetRatings(page: 4, pageSize: 20, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        Assert.Same(ratings, okResult.Value);
        Assert.Equal(4, service.LastRatingsPagination?.Page);
        Assert.Equal(20, service.LastRatingsPagination?.PageSize);
    }

    [Fact]
    public async Task AcceptRating_returns_no_content_after_moderation()
    {
        var ratingId = Guid.Parse("90909090-9090-9090-9090-909090909090");
        var moderationService = new FakeAdminRatingModerationService();
        var controller = new AdminController(
            new FakeAdminReadService(new AdminDashboardDto(0, 0, 0, 0, 0, 0, [])),
            moderationService);

        var response = await controller.AcceptRating(ratingId, CancellationToken.None);

        Assert.IsType<NoContentResult>(response);
        Assert.Equal(ratingId, moderationService.LastAcceptedRatingId);
    }

    [Fact]
    public async Task BanRating_returns_no_content_after_moderation()
    {
        var ratingId = Guid.Parse("91919191-9191-9191-9191-919191919191");
        var moderationService = new FakeAdminRatingModerationService();
        var controller = new AdminController(
            new FakeAdminReadService(new AdminDashboardDto(0, 0, 0, 0, 0, 0, [])),
            moderationService);

        var response = await controller.BanRating(
            ratingId,
            new BanBookRatingRequest("Spam izoh"),
            CancellationToken.None);

        Assert.IsType<NoContentResult>(response);
        Assert.Equal(ratingId, moderationService.LastBannedRatingId);
        Assert.Equal("Spam izoh", moderationService.LastBanReason);
    }

    private sealed class FakeAdminReadService(
        AdminDashboardDto dashboard,
        PagedResult<AdminBookDto>? books = null,
        PagedResult<AdminUserDto>? users = null,
        PagedResult<AdminBookRatingDto>? ratings = null) : IAdminReadService
    {
        public DateTimeOffset? LastFrom { get; private set; }

        public DateTimeOffset? LastTo { get; private set; }

        public PaginationQuery? LastBooksPagination { get; private set; }

        public PaginationQuery? LastUsersPagination { get; private set; }

        public PaginationQuery? LastRatingsPagination { get; private set; }

        public Task<PagedResult<AdminBookDto>> GetBooksAsync(
            PaginationQuery pagination,
            CancellationToken cancellationToken)
        {
            LastBooksPagination = pagination;
            return Task.FromResult(books ?? new PagedResult<AdminBookDto>([], pagination.Page, pagination.PageSize, 0));
        }

        public Task<PagedResult<AdminUserDto>> GetUsersAsync(
            PaginationQuery pagination,
            CancellationToken cancellationToken)
        {
            LastUsersPagination = pagination;
            return Task.FromResult(users ?? new PagedResult<AdminUserDto>([], pagination.Page, pagination.PageSize, 0));
        }

        public Task<PagedResult<AdminBookRatingDto>> GetRatingsAsync(
            PaginationQuery pagination,
            CancellationToken cancellationToken)
        {
            LastRatingsPagination = pagination;
            return Task.FromResult(ratings ?? new PagedResult<AdminBookRatingDto>([], pagination.Page, pagination.PageSize, 0));
        }

        public Task<AdminDashboardDto> GetDashboardAsync(
            DateTimeOffset? from,
            DateTimeOffset? to,
            CancellationToken cancellationToken)
        {
            LastFrom = from;
            LastTo = to;
            return Task.FromResult(dashboard);
        }
    }

    private sealed class FakeAdminRatingModerationService : IAdminRatingModerationService
    {
        public Guid? LastAcceptedRatingId { get; private set; }

        public Guid? LastBannedRatingId { get; private set; }

        public string? LastBanReason { get; private set; }

        public Task AcceptRatingAsync(Guid ratingId, CancellationToken cancellationToken)
        {
            LastAcceptedRatingId = ratingId;
            return Task.CompletedTask;
        }

        public Task BanRatingAsync(Guid ratingId, string? banReason, CancellationToken cancellationToken)
        {
            LastBannedRatingId = ratingId;
            LastBanReason = banReason;
            return Task.CompletedTask;
        }
    }
}
