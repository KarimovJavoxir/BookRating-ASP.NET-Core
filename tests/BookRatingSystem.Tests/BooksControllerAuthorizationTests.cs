using System.Reflection;
using BookRatingSystem.Api.Controllers;
using Microsoft.AspNetCore.Authorization;

namespace BookRatingSystem.Tests;

public sealed class BooksControllerAuthorizationTests
{
    [Theory]
    [InlineData(nameof(BooksController.CreateBook))]
    [InlineData(nameof(BooksController.UpdateBook))]
    [InlineData(nameof(BooksController.DeleteBook))]
    public void Book_management_actions_require_admin_policy(string actionName)
    {
        var authorizeAttribute = GetAuthorizeAttribute(actionName);

        Assert.Equal("AdminOnly", authorizeAttribute.Policy);
    }

    [Fact]
    public void SubmitRating_requires_authenticated_user_policy_only()
    {
        var authorizeAttribute = GetAuthorizeAttribute(nameof(BooksController.SubmitRating));

        Assert.Equal("AuthenticatedUser", authorizeAttribute.Policy);
    }

    private static AuthorizeAttribute GetAuthorizeAttribute(string actionName)
    {
        var method = typeof(BooksController).GetMethods()
            .Single(candidate => candidate.Name == actionName);

        return method.GetCustomAttribute<AuthorizeAttribute>()
            ?? throw new InvalidOperationException($"{actionName} does not have an Authorize attribute.");
    }
}
