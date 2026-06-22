namespace BookRatingSystem.Application.Books;

public sealed class BookSearchUnavailableException(string message, Exception innerException)
    : Exception(message, innerException);
