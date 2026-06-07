# SCM3.Api — Minimal API (built-in `MapGroup`)

> See the root `CLAUDE.md` for the full project plan. This file orients Claude Code
> specifically inside the API project. Project status: **6 endpoint modules in place**
> (`CustomerEndpoints`, `NotificationEndpoints`, `ProductEndpoints`, `RequestEndpoints`,
> `SystemEndpoints`, `UserEndpoints`) covering every `SCM3.Data` service. `IEmailService`
> and the Carter API endpoints item from the root plan are resolved — see below.

## Purpose

Per the binding architecture pivot (`Web -> Api -> Data -> DB`), `SCM3.Api` is the
**only** project that talks to the database — `SCM3.Web` reaches every domain service
over HTTP through the endpoint groups here (typed `HttpClient` wrappers registered in
`SCM3.Web/Program.cs`, resolved via Aspire service discovery as `https+http://api`).
Don't duplicate business rules here; call into `SCM3.Data` services exactly like the
old direct-reference path did.

## Conventions

- Endpoints are **`MapGroup` extension methods** living in `Endpoints/`, one static
  class + one `Map<Resource>Endpoints(this IEndpointRouteBuilder app)` per resource
  area: `CustomerEndpoints.cs`, `NotificationEndpoints.cs`, `ProductEndpoints.cs`,
  `RequestEndpoints.cs`, `SystemEndpoints.cs`, `UserEndpoints.cs`
- **We use the framework's built-in `app.MapGroup(...)` / `RouteGroupBuilder` —
  not Carter or `[ApiController]` classes.** The `Carter` package was removed; each
  module returns its `RouteGroupBuilder` so `Program.cs` can chain `.WithTags(...)` /
  auth policies on it later if needed. See `RequestEndpoints.MapRequestEndpoints` for
  the canonical shape (group + grouped sub-resource routes + a returned group).
- `Program.cs` calls each `app.Map<Resource>Endpoints()` explicitly (no module
  auto-discovery/registration step — keep the call list there in sync with `Endpoints/`)
- Auth: **same Windows Auth + cookie scheme as `SCM3.Web`** — root CLAUDE.md §3 explicitly
  notes there is no JWT in the current scope; don't introduce one without checking that decision
- Project references `SCM3.Data` (for entities/services) and `SCM3.ServiceDefaults`
  (Aspire telemetry/health-check wiring — keep in sync with `SCM3.Web`)

## Adding an endpoint module

1. Create `Endpoints/<Resource>Endpoints.cs`:
   ```csharp
   public static class WidgetEndpoints
   {
       public static RouteGroupBuilder MapWidgetEndpoints(this IEndpointRouteBuilder app)
       {
           var group = app.MapGroup("/api/widgets").WithTags("Widgets");

           group.MapGet("/", async (IWidgetService service)
               => Results.Ok(await service.GetAllAsync()));
           // ...

           return group;
       }
   }
   ```
2. Map routes against the relevant `SCM3.Data` service interface (don't reach into
   `DbContext`/repositories directly — go through the same service layer the UI uses)
3. Register it in `SCM3.Api/Program.cs` alongside the other `app.Map<Resource>Endpoints()` calls
4. Apply the same role/permission checks described in root CLAUDE.md §4 — the API is
   not exempt from the three-layer authorization model
