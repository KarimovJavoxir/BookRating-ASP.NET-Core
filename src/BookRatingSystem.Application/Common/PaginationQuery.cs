namespace BookRatingSystem.Application.Common;

public sealed record PaginationQuery
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 10;
    public const int MaxPageSize = 100;

    public PaginationQuery(int page = DefaultPage, int pageSize = DefaultPageSize)
    {
        Page = Math.Max(DefaultPage, page);
        PageSize = Math.Clamp(pageSize, 1, MaxPageSize);
    }

    public int Page { get; }

    public int PageSize { get; }

    public int Skip => (Page - 1) * PageSize;
}
