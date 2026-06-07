var builder = DistributedApplication.CreateBuilder(args);

// Redis — lookup cache for rarely-changing reference data (root CLAUDE.md §9) and the
// SignalR backplane once more than one SCM3.Web instance runs (root CLAUDE.md §12).
// Resource name "cache" is what SCM3.Web reads via
// builder.Configuration.GetConnectionString("cache") (see SCM3.Web/Program.cs).
var cache = builder.AddRedis("cache");

// SQL Server (Aspire.Hosting.SqlServer is already referenced — root CLAUDE.md §1 "dev
// on network / production") is intentionally NOT instantiated here yet: SCM3.Data's
// InitialCreate migration was generated with SQLite as the design-time provider and
// hardcodes SQLite column types (`type: "INTEGER"`/`type: "TEXT"`) on ~96 columns,
// including string primary/foreign keys (e.g. Users.Username). SQL Server's `text` type
// can't appear in WHERE/ORDER BY/JOIN/keys, so auto-migrating this verbatim would create
// a schema that breaks on the app's most basic queries (GetByUsernameAsync, any
// OrderBy(x => x.Name), etc.) — worse than not wiring it at all. Add it back once the
// migration is regenerated portably (omit explicit HasColumnType so each provider's own
// type mapper picks native types — see SCM3.Data/CLAUDE.md "SQLite <-> SQL Server swap").
// Until then SCM3.Api keeps its DataProvider="Sqlite" default and runs the fully-seeded,
// fully-tested repo-shipped scm3.db — exactly the "demo mode... don't make SQL Server a
// hard dependency of the AppHost graph" path this project's own CLAUDE.md calls for.

// SCM3.Api is the only project that talks to the database (Web -> Api -> Data -> DB,
// the binding architecture pivot). Resource name "api" is exactly the hostname
// SCM3.Web's default Api:BaseUrl ("https+http://api") resolves through Aspire service
// discovery — WithReference below wires that resolution up automatically.
var api = builder.AddProject<Projects.SCM3_Api>("api");

builder.AddProject<Projects.SCM3_Web>("web")
    .WithReference(cache)
    .WithReference(api)
    .WaitFor(cache)
    .WaitFor(api);

builder.Build().Run();
