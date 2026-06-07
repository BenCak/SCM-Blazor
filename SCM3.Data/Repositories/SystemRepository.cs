using Microsoft.EntityFrameworkCore;
using SCM3.Data.Entities;

namespace SCM3.Data.Repositories;

public class SystemRepository(DbContext.SCM3DbContext context) : ISystemRepository
{
    public async Task<List<SystemEntity>> GetAllAsync()
    {
        return await context.Systems
            .Include(s => s.Customer)
            .Include(s => s.Product)
            .Where(s => !s.IsDeleted)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<SystemEntity?> GetByIdAsync(int id)
    {
        return await context.Systems
            .Include(s => s.Customer)
            .Include(s => s.Product)
            .FirstOrDefaultAsync(s => s.SystemId == id && !s.IsDeleted);
    }

    public async Task<List<SystemEntity>> GetByCustomerAsync(int customerId)
    {
        return await context.Systems
            .Include(s => s.Product)
            .Where(s => s.CustomerId == customerId && !s.IsDeleted)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<List<SystemEntity>> GetByProductAsync(int productId)
    {
        return await context.Systems
            .Include(s => s.Customer)
            .Where(s => s.ProductId == productId && !s.IsDeleted)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task AddAsync(SystemEntity entity)
    {
        context.Systems.Add(entity);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(SystemEntity entity)
    {
        context.Systems.Update(entity);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await context.Systems.FindAsync(id);
        if (entity is null)
        {
            return;
        }

        entity.IsDeleted = true;
        await context.SaveChangesAsync();
    }
}
