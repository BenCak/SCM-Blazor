using SCM3.Data.Entities;
using SCM3.Data.Repositories;

namespace SCM3.Data.Services;

public class ProductService(IProductRepository repository) : IProductService
{
    public Task<List<Product>> GetAllAsync() => repository.GetAllAsync();

    public Task<Product?> GetByIdAsync(int id) => repository.GetByIdAsync(id);

    public Task SaveAsync(Product product)
        => product.ProductId == 0 ? repository.AddAsync(product) : repository.UpdateAsync(product);

    public Task DeleteAsync(int id) => repository.DeleteAsync(id);
}
