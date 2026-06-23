using BookRatingSystem.Application.Abstractions;
using BookRatingSystem.Application.Admin;
using BookRatingSystem.Infrastructure.Admin;
using BookRatingSystem.Infrastructure.Persistence;
using BookRatingSystem.Infrastructure.Repositories;
using BookRatingSystem.Infrastructure.Search;
using BookRatingSystem.Infrastructure.Security;
using BookRatingSystem.Infrastructure.Time;
using Meilisearch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BookRatingSystem.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        services.AddDbContext<BookRatingDbContext>(options => options.UseNpgsql(connectionString));
        services.Configure<MeilisearchOptions>(options =>
        {
            var section = configuration.GetSection(MeilisearchOptions.SectionName);

            options.Url = Environment.GetEnvironmentVariable("MEILISEARCH_URL")
                ?? section["Url"]
                ?? options.Url;
            options.ApiKey = Environment.GetEnvironmentVariable("MEILISEARCH_KEY")
                ?? section["ApiKey"]
                ?? options.ApiKey;
            options.BooksIndex = section["BooksIndex"] ?? options.BooksIndex;
            options.UsePostgresFallback = bool.TryParse(section["UsePostgresFallback"], out var usePostgresFallback)
                ? usePostgresFallback
                : options.UsePostgresFallback;
        });
        services.AddSingleton(provider =>
        {
            var options = provider.GetRequiredService<IOptions<MeilisearchOptions>>().Value;
            if (string.IsNullOrWhiteSpace(options.Url))
            {
                throw new InvalidOperationException("Meilisearch URL is not configured.");
            }

            if (string.IsNullOrWhiteSpace(options.ApiKey))
            {
                throw new InvalidOperationException("Meilisearch API key is not configured.");
            }

            if (string.IsNullOrWhiteSpace(options.BooksIndex))
            {
                throw new InvalidOperationException("Meilisearch books index name is not configured.");
            }

            return new MeilisearchClient(options.Url, options.ApiKey);
        });
        services.AddScoped<IBookRepository, EfBookRepository>();
        services.AddScoped<IUserRepository, EfUserRepository>();
        services.AddScoped<IAdminReadService, EfAdminReadService>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<BookRatingDbContext>());
        services.AddScoped<IPasswordHashService, Pbkdf2PasswordHashService>();
        services.AddScoped<PostgresBookSearchService>();
        services.AddScoped<IBookSearchService, MeilisearchBookSearchService>();
        services.AddScoped<IBookIndexingService, MeilisearchBookIndexingService>();
        services.AddSingleton<IClock, SystemClock>();

        return services;
    }
}
