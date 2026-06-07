using SCM3.Data.Entities;

namespace SCM3.Data.Repositories;

public interface ISystemRepository
{
    Task<List<SystemEntity>> GetAllAsync();
    Task<SystemEntity?> GetByIdAsync(int id);
    Task<List<SystemEntity>> GetByCustomerAsync(int customerId);
    Task<List<SystemEntity>> GetByProductAsync(int productId);
    Task AddAsync(SystemEntity entity);
    Task UpdateAsync(SystemEntity entity);
    Task DeleteAsync(int id); // soft delete — sets IsDeleted = true
}
