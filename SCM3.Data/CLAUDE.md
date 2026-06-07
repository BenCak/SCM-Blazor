# SCM3.Data — EF Core Data Layer

> See the root `CLAUDE.md` for the full project plan. This file orients Claude Code
> specifically inside the data layer. Project status: **entities, `SCM3DbContext`,
> initial migration, seeder, repositories, and service implementations are all in
> place; `scm3.db` ships pre-seeded in this folder**. `IEmailService`/`CacheService`/
> `ICurrentUserService` and the Carter API endpoints are still open — update this file
> as those land.

## What lives here

- `Entities/` — 14 POCO entity classes, every one carrying `IsDeleted` + `LogDate`
  (soft delete pattern, root CLAUDE.md §2). Note: the `System` table is modeled as
  `SystemEntity` (collides with `global::System`); its `DbSet` is still exposed as
  `Systems` and its primary key (`SystemId`) is configured explicitly in
  `OnModelCreating` since EF's convention can't infer it from the type name.
  - `Request` and `RequestSCMStatus` carry the **shared "spine" scalar columns** from
    the root CLAUDE.md §5 Per-Type UI Field Reference — fields common across artifact
    types (requestor snapshot, dates, descriptions, SCM reviewer/comment fields, etc.)
    live directly on these entities rather than in the per-type JSON blob. `Request`:
    `RequestorName`/`RequestorEmail`/`RequestorPhone` (point-in-time snapshot — kept on
    the request rather than read live off `User`, which has no `Phone`, so historical
    accuracy survives later account changes), `ParentVersion`, `ReadyDate`, `NeedDate`,
    `ReleaseDate`, `ReleaseDescription`, `NotesToScm` (plus pre-existing `ChargeNumber`/
    `Priority`/`SubmittedDate`). `RequestSCMStatus`: `ReviewerNotes`, `Comments`,
    `ReleaseReadinessNotes` (the last is unique to System Release; CLAUDE.md's SCM
    Status list also names Expected Completion Date/On Hold/Work Description/SCM Notes
    — those remain TBD, not yet columns on this entity).
  - `Entities/Attributes/` — six strongly-typed DTOs that give `RequestAttributes.
    CustomAttributes` a real shape per `RequestType` (namespace `SCM3.Data.Entities.
    Attributes`): `SystemReleaseAttributes`, `SegmentReleaseAttributes`,
    `AsiCsciReleaseAttributes`, `SupplierCsciReleaseAttributes`,
    `ThirdPartySoftwareAttributes`, `EeReleaseAttributes`. Each holds only the
    type-unique scalars and structured groups (Associations/Associated Items/DCNs/
    Common Libraries/Checksums/etc., from root CLAUDE.md §5) — fields already covered
    by the `Request`/`RequestSCMStatus` spine columns are deliberately omitted. Group
    rows are modeled as nested classes scoped per-DTO (e.g. `AsiCsciReleaseAttributes.
    DcnEntry` vs `SupplierCsciReleaseAttributes.DcnEntry`) rather than shared types, to
    avoid name collisions while keeping each shape self-contained. Naming quirks from
    the UI spec are preserved verbatim — e.g. `LrePartNumbers` on Supplier CSCI,
    distinct from `LruPartNumbers` elsewhere (not a typo to "fix"). TE Request has no
    DTO — its `CustomAttributes` shape predates this reference and remains TBD
    (root CLAUDE.md §5 / Open Item §17.1).
- `DbContext/` — `SCM3DbContext` (all 14 `DbSet`s + Fluent API relationship config;
  every FK is `DeleteBehavior.Restrict` since soft delete is the only delete path) and
  `SCM3DbContextFactory` (`IDesignTimeDbContextFactory`, pins SQLite as the design-time
  default so `dotnet ef` works without a startup project)
- `Migrations/` — EF Core migrations, don't hand-edit. `InitialCreate` (initial schema)
  then `AddRequestSpineAndScmStatusFields` (adds the 12 spine/SCM-status columns above
  to `Requests`/`RequestSCMStatuses` — the `Attributes` DTOs need no migration since
  `CustomAttributes` stays a single JSON `string` column)
- `Repositories/` — `IRequestRepository`/`RequestRepository` (owns the Request
  aggregate plus its sub-resources — attributes, notes, history, attachments, SCM
  status, change log — rather than splitting each into its own repo, since the detail
  page surfaces them all together), `ISystemRepository`/`SystemRepository`,
  `ICustomerRepository`/`CustomerRepository`, `IProductRepository`/`ProductRepository`,
  `INotificationRepository`/`NotificationRepository`. Each follows the
  `GetAll/ById/Add/Update/Delete` shape from the New Domain Entity Recipe (root
  CLAUDE.md §16), where `Delete` is always a **soft** delete (`IsDeleted = true`), and
  takes `SCM3DbContext` via primary-constructor injection.
- `Services/` — `IRequestService`/`RequestService`, `ISystemService`/`SystemService`,
  `ICustomerService`/`CustomerService`, `IProductService`/`ProductService`,
  `INotificationService`/`NotificationService`. Each is a thin wrapper over its matching
  repository (repository pattern — no direct `SCM3DbContext` access from services).
  **Deviation from root CLAUDE.md §9**: that section places interfaces in
  `SCM3.Web/Services` and implementations here, but `SCM3.Web` already references
  `SCM3.Data` — the reverse reference would be circular and won't build. Interfaces are
  defined alongside their implementations here instead; `SCM3.Web/Program.cs` registers
  them via its existing reference to this project. `ICurrentUserService`,
  `CacheService`, and `IEmailService` are not part of this layer (cookie claims, Redis,
  and SMTP concerns belong in `SCM3.Web`) and remain open.
- `Seed/` — `SCM3Seeder.SeedAsync` populates demo data the first time it's run against
  an empty db (guarded by `if (await context.Customers.AnyAsync()) return;`): 3
  customers, 3 products, 3 systems, 5 demo users, 7 request types, 6 statuses, 7 sample
  requests (one per type, forming the System → Segment → CSCI hierarchy plus 3
  standalone EE/TE/Third-Party requests), `CustomAttributes` JSON, notes, and history
  chains (root CLAUDE.md §2). The seeded `Request` rows now also populate the spine
  columns (requestor snapshot, ready/need/release dates, release description, notes to
  SCM, parent version), and the `CustomAttributes` JSON for the six types covered by
  root CLAUDE.md §5's Per-Type UI Field Reference now matches the shape of their
  `Entities/Attributes` DTO 1:1 (e.g. `AssociatedSegments`/`CompatibleSoftware` for
  System, `Dcns`/`CommonLibraries`/`LruPartNumbers` for ASI CSCI). TE Request keeps its
  pre-existing shape since it has no DTO yet.
  NOTE: root CLAUDE.md §2 says "6 Request Types + 8 Statuses", but only 7 types (§5) and
  6 statuses (§6) are concretely defined anywhere — the seeder uses those 7/6 rather than
  inventing two more of each (see Open Items §17.1/§17.7; worth reconciling in the plan).
  **`SeedAsync` is not currently invoked from `Program.cs`/`AppHost`/anywhere in the
  app** — it only runs when something calls it explicitly (e.g. a one-off bootstrap run
  used to produce `scm3.db`). If you change the seed shapes, you must regenerate
  `scm3.db` yourself (point a fresh `SCM3DbContext` at a new file, call
  `Database.MigrateAsync()` then `SCM3Seeder.SeedAsync(context)`, then replace the
  checked-in file) — editing `SCM3Seeder.cs` alone won't change what ships.
- `scm3.db` — the pre-seeded SQLite database (matches `SCM3DbContextFactory`'s
  `"Data Source=scm3.db"` connection string), so the app runs out of the box in demo
  mode. Regenerated against the current migrations + seed shapes described above —
  contains the `__EFMigrationsHistory` rows for both `InitialCreate` and
  `AddRequestSpineAndScmStatusFields`.

## SQLite ↔ SQL Server swap

The whole point of this layer is that **the rest of the app never knows which provider is
active**. A single `DataProvider` config flag (`Sqlite` | `SqlServer`) selects the
implementation registered in `SCM3.Web/Program.cs` (root CLAUDE.md §1):

```csharp
if (config["DataProvider"] == "Sqlite")
    builder.Services.AddScoped<IRequestService, SqliteRequestService>();
else
    builder.Services.AddScoped<IRequestService, SqlServerRequestService>();
```

Keep entity shapes, migrations, and Dapper SQL portable across both providers — avoid
provider-specific column types or functions unless strictly necessary, and isolate any
that are required behind the provider check above.

## Migrations workflow

```bash
dotnet ef migrations add <Name> --project SCM3.Data
dotnet ef database update --project SCM3.Data
```

## Adding a new entity

Don't improvise — follow the **New Domain Entity Recipe** in the root `CLAUDE.md` (§16).
It defines the exact sequence (entity → `DbSet` → migration → seed → repository → service
interface in `SCM3.Web` → service implementation here → DI → Blazor page → nav entry) using
`CsciName` as the worked example.

## Schema reference

The full table list, the `Customer → Product → System → Request` hierarchy, and the
per-request-type `CustomAttributes` JSON shapes are documented in root CLAUDE.md §2 and §5 —
consult those before adding or changing entities so the JSON shape stays consistent with
what the UI and validators expect.
