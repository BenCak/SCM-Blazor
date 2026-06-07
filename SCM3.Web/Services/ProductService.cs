using System.Net.Http.Json;
using SCM3.Data.Entities;

namespace SCM3.Web.Services;

public class ProductService(HttpClient http) : IProductService
{
    public async Task<List<Product>> GetAllAsync()
        => await http.GetFromJsonAsync<List<Product>>("/api/products") ?? [];

    public Task<Product?> GetByIdAsync(int id) => http.GetOrDefaultAsync<Product>($"/api/products/{id}");

    public async Task SaveAsync(Product product)
    {
        if (product.ProductId == 0)
        {
            var response = await http.PostAsJsonAsync("/api/products", product);
            response.EnsureSuccessStatusCode();
            var created = await response.Content.ReadFromJsonAsync<Product>();
            if (created is not null)
            {
                product.ProductId = created.ProductId;
            }
        }
        else
        {
            var response = await http.PutAsJsonAsync($"/api/products/{product.ProductId}", product);
            response.EnsureSuccessStatusCode();
        }
    }

    public async Task DeleteAsync(int id)
    {
        var response = await http.DeleteAsync($"/api/products/{id}");
        response.EnsureSuccessStatusCode();
    }
}
