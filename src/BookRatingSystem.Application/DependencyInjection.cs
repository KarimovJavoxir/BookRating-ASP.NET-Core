using BookRatingSystem.Application.Books;
using Microsoft.Extensions.DependencyInjection;

namespace BookRatingSystem.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IBookService, BookService>();

        return services;
    }
}
