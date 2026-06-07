using Microsoft.EntityFrameworkCore;
using SCM3.Data.Entities;

namespace SCM3.Data.Repositories;

public class ProductRepository(DbContext.SCM3DbContext context) : IProductRepository
{
    public async Task<List<Product>> GetAllAsync()
    {
        return await context.Products
            .Where(p => !p.IsDeleted)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await context.Products
            .FirstOrDefaultAsync(p => p.ProductId == id && !p.IsDeleted);
    }

    public async Task AddAsync(Product entity)
    {
        context.Products.Add(entity);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Product entity)
    {
        context.Products.Update(entity);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await context.Products.FindAsync(id);
        if (entity is null)
        {
            return;
        }

        entity.IsDeleted = true;
        await context.SaveChangesAsync();
    }
}
