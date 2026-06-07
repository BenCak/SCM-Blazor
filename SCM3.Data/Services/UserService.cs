using SCM3.Data.Entities;
using SCM3.Data.Repositories;

namespace SCM3.Data.Services;

public class UserService(IUserRepository repository) : IUserService
{
    public Task<List<User>> GetAllAsync() => repository.GetAllAsync();

    public Task<User?> GetByIdAsync(int id) => repository.GetByIdAsync(id);

    public Task<User?> GetByUsernameAsync(string username) => repository.GetByUsernameAsync(username);

    public Task SaveAsync(User user)
        => user.UserId == 0 ? repository.AddAsync(user) : repository.UpdateAsync(user);

    public Task DeleteAsync(int id) => repository.DeleteAsync(id);
}
