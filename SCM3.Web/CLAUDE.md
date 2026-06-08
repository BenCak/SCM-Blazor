# SCM3.Web — Blazor Server UI

> See the root `CLAUDE.md` for the full project plan. This file orients Claude Code
> specifically inside the Blazor UI project. Project status: **master-detail request
> dashboard, request detail surface, admin reference-data pages, and HTTP-backed
> service wrappers are in place**. Keep this file aligned with the current UI shell
> and interaction patterns as they evolve.

## What lives here

- `Components/Pages/` — routable pages (incl. `Admin/` for SCM DB Manager pages)
- `Components/Shared/` — reusable components (request cards, detail panels, accordion
  sections, slide-in panels, popups)
- `Components/Layout/` — `MainLayout`, `NavMenu`, drawer/header chrome
- `Services/` — **interfaces only** (`IRequestService`, `ISystemService`,
  `ICustomerService`, `IProductService`, `ICurrentUserService`, `CacheService`,
  `INotificationService`, `IEmailService`); implementations live in `SCM3.Data/Services/`
- `EmailTemplates/` — Razor-based HTML email templates, one per notification action
- `wwwroot/css/themes/` — Telerik ThemeBuilder exports (`scm3-gasi.css`, `scm3-gasi-dark.css`)

This project is built with `-int Server` (Blazor Server / interactive server rendering) —
do not introduce WebAssembly or Auto render modes; the architecture (SignalR, server-side
EF Core/Dapper access, cookie auth) assumes a server-rendered app.

## Render mode and rendering rules

- All interactive components run server-side; UI updates push over the existing SignalR circuit
- Telerik components require `Telerik.UI.for.Blazor` — currently resolved as a **trial**
  package (build emits `TKL105` license warnings). A commercial license key must be set
  via user secrets / environment variable before this ships (see root CLAUDE.md §17.4)
- Always use Telerik components for data rendering. Do not use raw HTML tables
  (`<table>`, `<thead>`, `<tbody>`, `<tr>`, `<td>`) for list/detail data surfaces.

## Three-layer authorization (root CLAUDE.md §3 / §4)

Every protected page/action enforces role checks at three layers — don't skip any:

1. Route: `@attribute [Authorize(Roles = "...")]`
2. UI: conditionally render buttons/actions/sections based on `ICurrentUserService.CanX()`
3. Service: re-check in the service implementation before mutating — the last line of defense

`ICurrentUserService` (reads claims from the auth cookie, populated from the `Users` table —
see root CLAUDE.md §3) is injected wherever role-aware behavior is needed.

## Master-detail layout (root CLAUDE.md §7)

The Release Artifacts experience is a **single-page master-detail** — selecting an item in
the left list loads the detail panel on the right with no full page navigation. The detail
panel is a header strip + actions bar + anchor-link section nav over a single scrollable
page of accordion sections. The left list uses a search box plus multi-select filter chips
for request type and workflow status; selecting a card updates the right-hand detail panel
in place. History and Change Log remain RHS slide-in panels triggered from the actions bar.

The detail surface renders five top-level sections: **Release Information**,
**Relationships**, **Attachments**, **SCM Status**, and **Audit Log**. The anchor links at
the top jump to those sections; they no longer switch hidden tab panes. Per-type fields in
Release Information / Relationships are rendered from the strongly typed attribute DTOs in
`SCM3.Data/Entities/Attributes`, while SCM Status and audit data come from the request
sub-resource endpoints exposed through `IRequestService`.

## Adding a new domain entity / admin page

Follow the **New Domain Entity Recipe** in the root `CLAUDE.md` (§16) end to end — it walks
through entity → DbContext → migration → seed → repository → service interface (here) →
service implementation (`SCM3.Data/Services/`) → DI registration → Blazor page → nav entry.

## Themes

Dynamic theme switching (no page reload) is wired through `App.razor`; the active theme is
persisted to `Users.Theme`. See root CLAUDE.md §13 for the CSS custom properties and file layout.
