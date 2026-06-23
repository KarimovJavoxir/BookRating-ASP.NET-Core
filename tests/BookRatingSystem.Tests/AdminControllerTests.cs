using BookRatingSystem.Api.Controllers;
using BookRatingSystem.Application.Admin;
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
        var controller = new AdminController(service);

        var response = await controller.GetDashboard(from, to, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        Assert.Same(dashboard, okResult.Value);
        Assert.Equal(from, service.LastFrom);
        Assert.Equal(to, service.LastTo);
    }

    [Fact]
    public async Task GetBooks_returns_all_admin_books_including_unverified()
    {
        var books = new[]
        {
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
        };
        var service = new FakeAdminReadService(
            new AdminDashboardDto(1, 0, 0, 1, 0, 0, []),
            books);
        var controller = new AdminController(service);

        var response = await controller.GetBooks(CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        Assert.Same(books, okResult.Value);
    }

    private sealed class FakeAdminReadService(
        AdminDashboardDto dashboard,
        IReadOnlyList<AdminBookDto>? books = null) : IAdminReadService
    {
        public DateTimeOffset? LastFrom { get; private set; }

        public DateTimeOffset? LastTo { get; private set; }

        public Task<IReadOnlyList<AdminBookDto>> GetBooksAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(books ?? []);
        }

        public Task<IReadOnlyList<AdminUserDto>> GetUsersAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<AdminUserDto>>([]);
        }

        public Task<IReadOnlyList<AdminBookRatingDto>> GetRatingsAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<AdminBookRatingDto>>([]);
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
}
