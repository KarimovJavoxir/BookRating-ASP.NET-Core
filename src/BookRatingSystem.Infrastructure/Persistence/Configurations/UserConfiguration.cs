using BookRatingSystem.Domain.Entities;
using BookRatingSystem.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookRatingSystem.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.Id)
            .ValueGeneratedNever();

        builder.Property(user => user.Username)
            .HasMaxLength(User.MaxUsernameLength)
            .IsRequired();

        builder.Property(user => user.Email)
            .HasMaxLength(User.MaxEmailLength)
            .IsRequired();

        builder.Property(user => user.PasswordHash)
            .HasMaxLength(User.MaxPasswordHashLength)
            .IsRequired();

        builder.Property(user => user.IsAdmin)
            .HasColumnName("is_admin")
            .IsRequired();

        builder.Property(user => user.CreatedAt)
            .IsRequired();

        builder.HasIndex(user => user.Username)
            .IsUnique();

        builder.HasIndex(user => user.Email)
            .IsUnique();

        var createdAt = new DateTimeOffset(2026, 6, 23, 0, 0, 0, TimeSpan.Zero);
        var users = Enumerable.Range(1, 20)
            .Select(index => new
            {
                Id = Guid.Parse($"10000000-0000-0000-0000-{index:000000000000}"),
                Username = $"user{index:00}",
                Email = $"user{index:00}@bookrate.uz",
                PasswordHash = Pbkdf2PasswordHashService.CreateSeedHash("User123!", $"bookrate-user-{index:00}"),
                IsAdmin = false,
                CreatedAt = createdAt
            })
            .Append(new
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000001"),
                Username = "admin",
                Email = "admin@bookrate.uz",
                PasswordHash = Pbkdf2PasswordHashService.CreateSeedHash("Admin123!", "bookrate-admin-01"),
                IsAdmin = true,
                CreatedAt = createdAt
            });

        builder.HasData(users);
    }
}
