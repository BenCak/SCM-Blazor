using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Authorization;
using Moq;
using SCM3.Data.Entities;
using SCM3.Web.Services;

namespace SCM3.Tests.Unit.Services;

public class CurrentUserServiceTests
{
    private static CurrentUserService CreateSut(User? user, out Mock<IUserService> userService)
    {
        var authProvider = new Mock<AuthenticationStateProvider>();
        var identity = user is null
            ? new ClaimsIdentity()
            : new ClaimsIdentity([new Claim("Username", user.Username)], "TestAuth");
        authProvider
            .Setup(p => p.GetAuthenticationStateAsync())
            .ReturnsAsync(new AuthenticationState(new ClaimsPrincipal(identity)));

        userService = new Mock<IUserService>();
        if (user is not null)
        {
            userService.Setup(s => s.GetByUsernameAsync(user.Username)).ReturnsAsync(user);
        }

        return new CurrentUserService(authProvider.Object, userService.Object);
    }

    private static async Task<CurrentUserService> CreateLoadedSutAsync(string role)
    {
        var user = new User { UserId = 1, Username = "testuser", FullName = "Test User", Role = role };
        var sut = CreateSut(user, out _);
        await sut.LoadAsync();
        return sut;
    }

    [Theory]
    [InlineData("SCM_Admin", true)]
    [InlineData("SCM_Staff", true)]
    [InlineData("Viewer", false)]
    public async Task IsInRole_MatchesCurrentUsersRole(string role, bool expected)
    {
        var sut = await CreateLoadedSutAsync(role);

        sut.IsInRole("SCM_Staff", "SCM_Admin").Should().Be(expected);
    }

    [Fact]
    public void IsInRole_ReturnsFalse_WhenNoUserLoaded()
    {
        var sut = CreateSut(null, out _);

        sut.IsInRole("Viewer").Should().BeFalse();
    }

    [Theory]
    [InlineData("SCM_Admin")]
    [InlineData("SCM_Staff")]
    public async Task CanEdit_ReturnsTrue_ForScmRoles_RegardlessOfOwnershipOrStatus(string role)
    {
        var sut = await CreateLoadedSutAsync(role);
        var request = new Request { RequestorUserId = 999, RequestStatus = new RequestStatus { Name = "Released" } };

        sut.CanEdit(request).Should().BeTrue();
    }

    [Theory]
    [InlineData("TE_User")]
    [InlineData("EE_User")]
    public async Task CanEdit_ReturnsTrue_ForOwnerOfOwnDraftRequest(string role)
    {
        var sut = await CreateLoadedSutAsync(role);
        var request = new Request { RequestorUserId = 1, RequestStatus = new RequestStatus { Name = "Draft" } };

        sut.CanEdit(request).Should().BeTrue();
    }

    [Fact]
    public async Task CanEdit_ReturnsFalse_WhenOwnerButRequestIsNotDraft()
    {
        var sut = await CreateLoadedSutAsync("TE_User");
        var request = new Request { RequestorUserId = 1, RequestStatus = new RequestStatus { Name = "Pending" } };

        sut.CanEdit(request).Should().BeFalse();
    }

    [Fact]
    public async Task CanEdit_ReturnsFalse_WhenDraftButNotTheOwner()
    {
        var sut = await CreateLoadedSutAsync("TE_User");
        var request = new Request { RequestorUserId = 2, RequestStatus = new RequestStatus { Name = "Draft" } };

        sut.CanEdit(request).Should().BeFalse();
    }

    [Fact]
    public async Task CanEdit_ReturnsFalse_ForViewer()
    {
        var sut = await CreateLoadedSutAsync("Viewer");
        var request = new Request { RequestorUserId = 1, RequestStatus = new RequestStatus { Name = "Draft" } };

        sut.CanEdit(request).Should().BeFalse();
    }

    [Fact]
    public void CanEdit_ReturnsFalse_WhenNoUserLoaded()
    {
        var sut = CreateSut(null, out _);

        sut.CanEdit(new Request()).Should().BeFalse();
    }

    [Theory]
    [InlineData("SCM_Admin", true)]
    [InlineData("SCM_Staff", false)]
    [InlineData("Viewer", false)]
    public async Task CanApprove_IsTrueOnlyForScmAdmin(string role, bool expected)
    {
        var sut = await CreateLoadedSutAsync(role);

        sut.CanApprove().Should().Be(expected);
    }

    [Theory]
    [InlineData("SCM_Admin", true)]
    [InlineData("SCM_Staff", false)]
    [InlineData("Viewer", false)]
    public async Task CanRelease_IsTrueOnlyForScmAdmin(string role, bool expected)
    {
        var sut = await CreateLoadedSutAsync(role);

        sut.CanRelease().Should().Be(expected);
    }

    [Fact]
    public async Task LoadAsync_OnlyResolvesUserOnce_AcrossMultipleCalls()
    {
        var user = new User { UserId = 1, Username = "testuser", Role = "SCM_Admin" };
        var sut = CreateSut(user, out var userService);

        await sut.LoadAsync();
        await sut.LoadAsync();

        userService.Verify(s => s.GetByUsernameAsync(user.Username), Times.Once);
    }

    [Fact]
    public async Task LoadAsync_LeavesCurrentUserNull_WhenPrincipalHasNoUsernameClaim()
    {
        var sut = CreateSut(null, out var userService);

        await sut.LoadAsync();

        sut.CurrentUser.Should().BeNull();
        userService.Verify(s => s.GetByUsernameAsync(It.IsAny<string>()), Times.Never);
    }
}
