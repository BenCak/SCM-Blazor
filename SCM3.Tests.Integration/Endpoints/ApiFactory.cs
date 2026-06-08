using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using SCM3.Data.DbContext;

namespace SCM3.Tests.Integration.Endpoints;

// Boots SCM3.Api (Program is exposed as a partial class for this purpose) with its
// SQLite/SqlServer DbContext registration swapped for a fresh per-instance InMemory
// database, so endpoint tests exercise the real Minimal API + service + repository
// stack without touching scm3.db (root CLAUDE.md §18 "API endpoints" integration tests).
public class ApiFactory : WebApplicationFactory<Program>
{
    public string DatabaseName { get; } = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // SCM3.Api's own AddDbContext<SCM3DbContext>(UseSqlite/UseSqlServer) registration
            // has to be fully unwound — not just DbContextOptions<T> — otherwise EF layers our
            // UseInMemoryDatabase configuration on top of the original provider's and throws
            // "services for database providers X and Y have been registered" at first use.
            services.RemoveAll(typeof(IDbContextOptionsConfiguration<SCM3DbContext>));
            services.RemoveAll<DbContextOptions<SCM3DbContext>>();
            services.RemoveAll<DbContextOptions>();
            services.RemoveAll<SCM3DbContext>();

            services.AddDbContext<SCM3DbContext>(options => options.UseInMemoryDatabase(DatabaseName));
        });
    }

    public SCM3DbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<SCM3DbContext>()
            .UseInMemoryDatabase(DatabaseName)
            .Options;

        return new SCM3DbContext(options);
    }
}
