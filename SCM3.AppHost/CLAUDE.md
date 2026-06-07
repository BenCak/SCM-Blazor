# SCM3.AppHost — Aspire Orchestrator

> See the root `CLAUDE.md` for the full project plan. This file orients Claude Code
> specifically inside the orchestration project. Project status: **scaffold only —
> default Aspire AppHost, no resources wired yet**.

## Purpose

`Program.cs` here is the single place that wires together every running piece of SCM3 for
local development: the Blazor app, the API, and their backing services (Redis, SQL Server
container). It references `SCM3.Web` and `SCM3.Api` as launchable projects.

## How to add a resource (e.g. Redis, SQL Server)

Packages `Aspire.Hosting.Redis` and `Aspire.Hosting.SqlServer` are already installed.
Wire them up in `Program.cs` and pass references down to the projects that need them —
this is exactly how the **SignalR Redis backplane** gets configured (root CLAUDE.md §12):

```csharp
var redis = builder.AddRedis("cache");
var sql   = builder.AddSqlServer("sql").AddDatabase("SCM3");

builder.AddProject<Projects.SCM3_Web>("web")
    .WithReference(redis)
    .WithReference(sql);

builder.AddProject<Projects.SCM3_Api>("api")
    .WithReference(redis)
    .WithReference(sql);
```

`WithReference` is what makes the connection strings/service-discovery info show up in the
referencing project's configuration — that's how `SCM3.Web`/`SCM3.Api` find Redis and SQL
Server without hardcoded connection strings in dev.

## Why Redis matters here specifically

Redis serves **two** roles in this architecture (root CLAUDE.md §9, §12):
1. **Lookup cache** — `RequestType`, `RequestStatus`, `Customer`, `Product`, dashboard stats
2. **SignalR backplane** — required once more than one server instance is running, so that
   real-time updates (status changes, notes, notification bell, dashboard stats) reach every
   connected client regardless of which instance they're attached to

## Demo mode

When `DataProvider = "Sqlite"` (root CLAUDE.md §1, §2), the app runs against the
repo-shipped `.db` file and needs no SQL Server container — useful for stakeholder demos
without Docker/secrets. Don't make the SQL Server resource a hard dependency of the AppHost
graph if you want that mode to keep working without Docker running.

## Dashboard

Running `dotnet run --project SCM3.AppHost` opens the Aspire dashboard automatically —
this is the primary place to inspect logs (Serilog → console sink feeds it), traces, and
the health of every wired resource.
