using Microsoft.AspNetCore.Authorization;
using System.Reflection;

namespace BookRatingSystem.Tests;

public sealed class AdminControllerAuthorizationTests
{
    [Theory]
    [InlineData("GetBooks")]
    [InlineData("GetUsers")]
    [InlineData("GetRatings")]
    [InlineData("GetDashboard")]
    [InlineData("AcceptRating")]
    [InlineData("BanRating")]
    public void Admin_read_actions_require_admin_policy(string actionName)
    {
        var controllerType = Type.GetType(
            "BookRatingSystem.Api.Controllers.AdminController, BookRatingSystem.Api",
            throwOnError: false);

        Assert.NotNull(controllerType);

        var method = controllerType!.GetMethod(actionName, BindingFlags.Instance | BindingFlags.Public);
        Assert.NotNull(method);

        var authorizeAttribute = method!.GetCustomAttribute<AuthorizeAttribute>()
            ?? throw new InvalidOperationException($"{actionName} does not have an Authorize attribute.");

        Assert.Equal("AdminOnly", authorizeAttribute.Policy);
    }
}
