using System.Net.Http.Json;
using SCM3.Data.Entities;

namespace SCM3.Web.Services;

public class CustomerService(HttpClient http) : ICustomerService
{
    public async Task<List<Customer>> GetAllAsync()
        => await http.GetFromJsonAsync<List<Customer>>("/api/customers") ?? [];

    public Task<Customer?> GetByIdAsync(int id) => http.GetOrDefaultAsync<Customer>($"/api/customers/{id}");

    public async Task SaveAsync(Customer customer)
    {
        if (customer.CustomerId == 0)
        {
            var response = await http.PostAsJsonAsync("/api/customers", customer);
            response.EnsureSuccessStatusCode();
            var created = await response.Content.ReadFromJsonAsync<Customer>();
            if (created is not null)
            {
                customer.CustomerId = created.CustomerId;
            }
        }
        else
        {
            var response = await http.PutAsJsonAsync($"/api/customers/{customer.CustomerId}", customer);
            response.EnsureSuccessStatusCode();
        }
    }

    public async Task DeleteAsync(int id)
    {
        var response = await http.DeleteAsync($"/api/customers/{id}");
        response.EnsureSuccessStatusCode();
    }
}
