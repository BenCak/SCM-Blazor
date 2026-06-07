using SCM3.Data.Entities;
using SCM3.Data.Repositories;

namespace SCM3.Data.Services;

public class CustomerService(ICustomerRepository repository) : ICustomerService
{
    public Task<List<Customer>> GetAllAsync() => repository.GetAllAsync();

    public Task<Customer?> GetByIdAsync(int id) => repository.GetByIdAsync(id);

    public Task SaveAsync(Customer customer)
        => customer.CustomerId == 0 ? repository.AddAsync(customer) : repository.UpdateAsync(customer);

    public Task DeleteAsync(int id) => repository.DeleteAsync(id);
}
