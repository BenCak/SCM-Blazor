using SCM3.Data.Entities;

namespace SCM3.Web.Services;

// Logged-in user context + role checks, backed by claims on the auth cookie
// (root CLAUDE.md §3). NOTE: §3 declares `IsInRole(string role)` but the nav example in
// §16 calls `currentUser.IsInRole("SCM_Staff", "SCM_Admin")` — `params` satisfies both.
public interface ICurrentUserService
{
    User? CurrentUser { get; }
    string Role { get; }
    bool IsInRole(params string[] roles);
    bool CanEdit(Request request);
    bool CanApprove();
    bool CanRelease();

    // Loads CurrentUser from the authenticated principal's claims. Blazor Server can't
    // resolve the auth cookie synchronously at DI-construction time, so callers (layout/
    // pages) await this once per circuit before relying on the synchronous members above.
    Task LoadAsync();
}
