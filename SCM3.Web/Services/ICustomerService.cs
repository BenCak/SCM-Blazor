using SCM3.Data.Entities;

namespace SCM3.Web.Services;

// Customer lookups (root CLAUDE.md §9) — backs the Customer reference data managed
// under SCM DB Manager and used when composing Systems. HTTP-client-backed wrapper
// around SCM3.Api's /api/customers endpoint group.
public interface ICustomerService
{
    Task<List<Customer>> GetAllAsync();
    Task<Customer?> GetByIdAsync(int id);
    Task SaveAsync(Customer customer);
    Task DeleteAsync(int id);
}
