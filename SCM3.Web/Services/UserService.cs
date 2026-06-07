using System.Net.Http.Json;
using SCM3.Data.Entities;

namespace SCM3.Web.Services;

public class UserService(HttpClient http) : IUserService
{
    public async Task<List<User>> GetAllAsync()
        => await http.GetFromJsonAsync<List<User>>("/api/users") ?? [];

    public Task<User?> GetByIdAsync(int id) => http.GetOrDefaultAsync<User>($"/api/users/{id}");

    public Task<User?> GetByUsernameAsync(string username) => http.GetOrDefaultAsync<User>($"/api/users/by-username/{Uri.EscapeDataString(username)}");

    public async Task SaveAsync(User user)
    {
        if (user.UserId == 0)
        {
            var response = await http.PostAsJsonAsync("/api/users", user);
            response.EnsureSuccessStatusCode();
            var created = await response.Content.ReadFromJsonAsync<User>();
            if (created is not null)
            {
                user.UserId = created.UserId;
            }
        }
        else
        {
            var response = await http.PutAsJsonAsync($"/api/users/{user.UserId}", user);
            response.EnsureSuccessStatusCode();
        }
    }

    public async Task DeleteAsync(int id)
    {
        var response = await http.DeleteAsync($"/api/users/{id}");
        response.EnsureSuccessStatusCode();
    }
}
