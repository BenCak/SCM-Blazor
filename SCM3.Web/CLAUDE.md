# SCM3.Web — Blazor Server UI

> See the root `CLAUDE.md` for the full project plan. This file orients Claude Code
> specifically inside the Blazor UI project. Project status: **scaffold only — no
> application code yet**. Update this file as real components/services land.

## What lives here

- `Components/Pages/` — routable pages (incl. `Admin/` for SCM DB Manager pages)
- `Components/Shared/` — reusable components (detail panels, tab content, popups)
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

## Three-layer authorization (root CLAUDE.md §3 / §4)

Every protected page/action enforces role checks at three layers — don't skip any:

1. Route: `@attribute [Authorize(Roles = "...")]`
2. UI: conditionally render buttons/tabs based on `ICurrentUserService.CanX()`
3. Service: re-check in the service implementation before mutating — the last line of defense

`ICurrentUserService` (reads claims from the auth cookie, populated from the `Users` table —
see root CLAUDE.md §3) is injected wherever role-aware behavior is needed.

## Master-detail layout (root CLAUDE.md §7)

The Release Artifacts experience is a **single-page master-detail** — selecting an item in
the left list loads the detail panel on the right with no full page navigation. The detail
panel is a header strip + actions bar + tab strip, where each tab holds collapsible accordion
sections with **inline editing** (no separate Edit mode/button).

## Adding a new domain entity / admin page

Follow the **New Domain Entity Recipe** in the root `CLAUDE.md` (§16) end to end — it walks
through entity → DbContext → migration → seed → repository → service interface (here) →
service implementation (`SCM3.Data/Services/`) → DI registration → Blazor page → nav entry.

## Themes

Dynamic theme switching (no page reload) is wired through `App.razor`; the active theme is
persisted to `Users.Theme`. See root CLAUDE.md §13 for the CSS custom properties and file layout.
