using SCM3.Data.Entities;

namespace SCM3.Data.Services;

// Product lookups (root CLAUDE.md §9) — backs the Product reference data managed
// under SCM DB Manager and used when composing Systems.
public interface IProductService
{
    Task<List<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(int id);
    Task SaveAsync(Product product);
    Task DeleteAsync(int id);
}
