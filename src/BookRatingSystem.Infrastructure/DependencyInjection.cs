using BookRatingSystem.Application.Abstractions;
using BookRatingSystem.Infrastructure.Persistence;
using BookRatingSystem.Infrastructure.Repositories;
using BookRatingSystem.Infrastructure.Search;
using BookRatingSystem.Infrastructure.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookRatingSystem.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        services.AddDbContext<BookRatingDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IBookRepository, EfBookRepository>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<BookRatingDbContext>());
        services.AddScoped<IBookSearchService, PostgresBookSearchService>();
        services.AddSingleton<IClock, SystemClock>();

        return services;
    }
}
