using SCM3.Data.Entities;

namespace SCM3.Data.Repositories;

public interface ICustomerRepository
{
    Task<List<Customer>> GetAllAsync();
    Task<Customer?> GetByIdAsync(int id);
    Task AddAsync(Customer entity);
    Task UpdateAsync(Customer entity);
    Task DeleteAsync(int id); // soft delete — sets IsDeleted = true
}
