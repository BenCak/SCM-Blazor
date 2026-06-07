using SCM3.Data.Entities;

namespace SCM3.Web.Services;

// Backs the demo login lookup, ICurrentUserService's claims resolution, and
// Admin > Manage Users (root CLAUDE.md §3, §8). HTTP-client-backed wrapper around
// SCM3.Api's /api/users endpoint group.
public interface IUserService
{
    Task<List<User>> GetAllAsync();
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByUsernameAsync(string username);
    Task SaveAsync(User user);
    Task DeleteAsync(int id);
}
