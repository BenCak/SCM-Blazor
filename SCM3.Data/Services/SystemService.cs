using SCM3.Data.Entities;
using SCM3.Data.Repositories;

namespace SCM3.Data.Services;

public class SystemService(ISystemRepository repository) : ISystemService
{
    public Task<List<SystemEntity>> GetAllAsync() => repository.GetAllAsync();

    public Task<SystemEntity?> GetByIdAsync(int id) => repository.GetByIdAsync(id);

    public Task<List<SystemEntity>> GetByCustomerAsync(int customerId) => repository.GetByCustomerAsync(customerId);

    public Task<List<SystemEntity>> GetByProductAsync(int productId) => repository.GetByProductAsync(productId);

    public Task SaveAsync(SystemEntity system)
        => system.SystemId == 0 ? repository.AddAsync(system) : repository.UpdateAsync(system);

    public Task DeleteAsync(int id) => repository.DeleteAsync(id);
}
