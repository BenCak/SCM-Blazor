using Microsoft.AspNetCore.Components.Authorization;
using SCM3.Data.Entities;

namespace SCM3.Web.Services;

public class CurrentUserService(AuthenticationStateProvider authStateProvider, IUserService userService) : ICurrentUserService
{
    public User? CurrentUser { get; private set; }

    public string Role => CurrentUser?.Role ?? string.Empty;

    public bool IsInRole(params string[] roles) =>
        CurrentUser is not null && roles.Any(role => string.Equals(role, Role, StringComparison.OrdinalIgnoreCase));

    public bool CanEdit(Request request)
    {
        if (CurrentUser is null)
        {
            return false;
        }

        // SCM_Staff/SCM_Admin can edit anything; TE_User/EE_User only their own Draft
        // requests; Viewer can never edit (root CLAUDE.md §4 permission matrix).
        if (IsInRole("SCM_Staff", "SCM_Admin"))
        {
            return true;
        }

        return IsInRole("TE_User", "EE_User")
            && request.RequestorUserId == CurrentUser.UserId
            && request.RequestStatus?.Name == "Draft";
    }

    public bool CanApprove() => IsInRole("SCM_Admin");

    public bool CanRelease() => IsInRole("SCM_Admin");

    public async Task LoadAsync()
    {
        if (CurrentUser is not null)
        {
            return;
        }

        var authState = await authStateProvider.GetAuthenticationStateAsync();
        var username = authState.User.FindFirst("Username")?.Value;
        if (string.IsNullOrWhiteSpace(username))
        {
            return;
        }

        CurrentUser = await userService.GetByUsernameAsync(username);
    }
}
