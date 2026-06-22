namespace BookRatingSystem.Application.Books;

public sealed class BookNotFoundException(Guid bookId) : Exception($"Book with id '{bookId}' was not found.")
{
    public Guid BookId { get; } = bookId;
}
