using BookRatingSystem.Application.Abstractions;
using BookRatingSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookRatingSystem.Infrastructure.Persistence;

public sealed class BookRatingDbContext(DbContextOptions<BookRatingDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<Book> Books => Set<Book>();

    public DbSet<BookRating> BookRatings => Set<BookRating>();

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BookRatingDbContext).Assembly);
    }
}
