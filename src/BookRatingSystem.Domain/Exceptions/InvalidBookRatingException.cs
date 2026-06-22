namespace BookRatingSystem.Domain.Exceptions;

public sealed class InvalidBookRatingException(string message) : Exception(message);
