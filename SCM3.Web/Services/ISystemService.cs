using SCM3.Data.Entities;

namespace SCM3.Web.Services;

// System hierarchy queries (root CLAUDE.md §9) — Customer + Product -> System ->
// Request/SystemVersion -> Segment -> CSCI (root CLAUDE.md §2). HTTP-client-backed
// wrapper around SCM3.Api's /api/systems endpoint group.
public interface ISystemService
{
    Task<List<SystemEntity>> GetAllAsync();
    Task<SystemEntity?> GetByIdAsync(int id);
    Task<List<SystemEntity>> GetByCustomerAsync(int customerId);
    Task<List<SystemEntity>> GetByProductAsync(int productId);
    Task SaveAsync(SystemEntity system);
    Task DeleteAsync(int id);
}
