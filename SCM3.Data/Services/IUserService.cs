using SCM3.Data.Entities;

namespace SCM3.Data.Services;

// Backs UserEndpoints (SCM3.Api) — the demo login lookup, ICurrentUserService's claims
// resolution, and Admin > Manage Users (root CLAUDE.md §3, §8).
public interface IUserService
{
    Task<List<User>> GetAllAsync();
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByUsernameAsync(string username);
    Task SaveAsync(User user);
    Task DeleteAsync(int id);
}
