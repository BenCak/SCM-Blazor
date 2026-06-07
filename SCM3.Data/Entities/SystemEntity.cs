namespace SCM3.Data.Entities;

// Named "SystemEntity" rather than "System" — "System" collides with the global::System
// namespace and would force every reference in this file (and DbContext config) to be
// fully qualified. The DbSet/table is still named "Systems" to match the schema in
// root CLAUDE.md §2 (Customer + Product combination, e.g. "Army Gray Eagle").
public class SystemEntity
{
    public int SystemId { get; set; }
    public string Name { get; set; } = string.Empty;

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime LogDate { get; set; } = DateTime.UtcNow;
}
