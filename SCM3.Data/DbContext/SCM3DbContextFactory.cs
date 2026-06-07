using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SCM3.Data.DbContext;

// Lets `dotnet ef` create migrations without a startup project, and pins SQLite as the
// default provider for design-time/demo use (root CLAUDE.md §1 — SQLite ships with the
// repo for demo mode; SQL Server is swapped in at runtime via the DataProvider config flag).
public class SCM3DbContextFactory : IDesignTimeDbContextFactory<SCM3DbContext>
{
    public SCM3DbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SCM3DbContext>();
        optionsBuilder.UseSqlite("Data Source=scm3.db");

        return new SCM3DbContext(optionsBuilder.Options);
    }
}
