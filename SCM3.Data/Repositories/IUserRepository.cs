using SCM3.Data.Entities;

namespace SCM3.Data.Repositories;

// Needed by UserEndpoints (SCM3.Api) for the demo login lookup and admin user
// management — the Users table backs both the auth-cookie claims and the role/
// permission checks throughout the app (root CLAUDE.md §3).
public interface IUserRepository
{
    Task<List<User>> GetAllAsync();
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByUsernameAsync(string username);
    Task AddAsync(User entity);
    Task UpdateAsync(User entity);
    Task DeleteAsync(int id); // soft delete — sets IsDeleted = true
}
