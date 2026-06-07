using Microsoft.EntityFrameworkCore;
using SCM3.Data.Entities;

namespace SCM3.Data.Repositories;

public class CustomerRepository(DbContext.SCM3DbContext context) : ICustomerRepository
{
    public async Task<List<Customer>> GetAllAsync()
    {
        return await context.Customers
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Customer?> GetByIdAsync(int id)
    {
        return await context.Customers
            .FirstOrDefaultAsync(c => c.CustomerId == id && !c.IsDeleted);
    }

    public async Task AddAsync(Customer entity)
    {
        context.Customers.Add(entity);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Customer entity)
    {
        context.Customers.Update(entity);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await context.Customers.FindAsync(id);
        if (entity is null)
        {
            return;
        }

        entity.IsDeleted = true;
        await context.SaveChangesAsync();
    }
}
