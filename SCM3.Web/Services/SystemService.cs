using System.Net.Http.Json;
using SCM3.Data.Entities;

namespace SCM3.Web.Services;

public class SystemService(HttpClient http) : ISystemService
{
    public async Task<List<SystemEntity>> GetAllAsync()
        => await http.GetFromJsonAsync<List<SystemEntity>>("/api/systems") ?? [];

    public Task<SystemEntity?> GetByIdAsync(int id) => http.GetOrDefaultAsync<SystemEntity>($"/api/systems/{id}");

    public async Task<List<SystemEntity>> GetByCustomerAsync(int customerId)
        => await http.GetFromJsonAsync<List<SystemEntity>>($"/api/systems/by-customer/{customerId}") ?? [];

    public async Task<List<SystemEntity>> GetByProductAsync(int productId)
        => await http.GetFromJsonAsync<List<SystemEntity>>($"/api/systems/by-product/{productId}") ?? [];

    public async Task SaveAsync(SystemEntity system)
    {
        if (system.SystemId == 0)
        {
            var response = await http.PostAsJsonAsync("/api/systems", system);
            response.EnsureSuccessStatusCode();
            var created = await response.Content.ReadFromJsonAsync<SystemEntity>();
            if (created is not null)
            {
                system.SystemId = created.SystemId;
            }
        }
        else
        {
            var response = await http.PutAsJsonAsync($"/api/systems/{system.SystemId}", system);
            response.EnsureSuccessStatusCode();
        }
    }

    public async Task DeleteAsync(int id)
    {
        var response = await http.DeleteAsync($"/api/systems/{id}");
        response.EnsureSuccessStatusCode();
    }
}
