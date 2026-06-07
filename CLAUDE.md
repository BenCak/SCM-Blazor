# SCM3 Portal — Project Planning Document

> **General Atomics Aeronautical Systems, Inc. (GA-ASI)**
> Software Configuration Management Portal — Next Generation
> Stack: .NET 10 · Blazor Server · Telerik UI · Aspire · SQLite → SQL Server · Redis · SignalR · Serilog

---

## Table of Contents

1. [Solution Structure](#1-solution-structure)
2. [Database](#2-database)
3. [Security & Authentication](#3-security--authentication)
4. [Roles & Permissions](#4-roles--permissions)
5. [Request Types](#5-request-types)
6. [Request Workflow](#6-request-workflow)
7. [UI / UX](#7-ui--ux)
8. [Navigation](#8-navigation)
9. [Services](#9-services)
10. [Notifications & Email](#10-notifications--email)
11. [Validation](#11-validation)
12. [Real-Time (SignalR)](#12-real-time-signalr)
13. [Themes](#13-themes)
14. [Logging](#14-logging)
15. [CLAUDE.md Files](#15-claudemd-files)
16. [New Domain Entity Recipe](#16-new-domain-entity-recipe)
17. [Open Items / TBD](#17-open-items--tbd)

---

## 1. Solution Structure

```
SCM3.sln
├── SCM3.AppHost/              ← Aspire orchestrator — wires all services + Docker containers
├── SCM3.ServiceDefaults/      ← Shared Aspire config — Serilog, OpenTelemetry, health checks
├── SCM3.Web/                  ← Blazor Server app — UI, components, pages
├── SCM3.Api/                  ← Minimal API
└── SCM3.Data/                 ← EF Core data layer — SQLite (dev) → SQL Server (prod)
```

### Project Responsibilities

| Project                | Responsibility                                                        |
| ---------------------- | --------------------------------------------------------------------- |
| `SCM3.AppHost`         | Aspire orchestration — starts Web, Api, Redis, SQL Server containers  |
| `SCM3.ServiceDefaults` | Shared middleware — Serilog, OpenTelemetry, resilience, health checks |
| `SCM3.Web`             | Blazor Server UI — components, pages, layout, services interfaces     |
| `SCM3.Api`             | Minimal API — endpoint modules, future external/mobile consumers      |
| `SCM3.Data`            | EF Core DbContext, entities, repositories, migrations, seeder         |

### Developer Onboarding (one-time setup)

```bash
# 1. Clone
git clone https://gitlab.ga-asi.internal/scm/scm3.git
cd scm3

# 2. Set user secrets
dotnet user-secrets set "ConnectionStrings:SCM3" "..." --project SCM3.Web
dotnet user-secrets set "DataProvider" "SqlServer" --project SCM3.Web
dotnet user-secrets set "Smtp:Host" "mail.ga-asi.internal" --project SCM3.Web

# 3. Run everything
dotnet run --project SCM3.AppHost

# → Aspire dashboard opens automatically
# → Blazor app, Redis, SQL Server all running
# → Demo mode: set DataProvider = "Sqlite" — no secrets needed
```

### Data Provider Strategy

The app switches between SQLite and SQL Server via a single config flag.
Blazor components never know the difference — they only inject interfaces.

```json
// appsettings.json
{
  "DataProvider": "Sqlite"     // demo / stakeholder
  "DataProvider": "SqlServer"  // dev on network / production
}
```

```csharp
// Program.cs — one line swap
if (config["DataProvider"] == "Sqlite")
    builder.Services.AddScoped<IRequestService, SqliteRequestService>();
else
    builder.Services.AddScoped<IRequestService, SqlServerRequestService>();
```

---

## 2. Database

### Technology Stack

| Component                  | Dev / Demo                           | Production                     |
| -------------------------- | ------------------------------------ | ------------------------------ |
| Database                   | SQLite (`.db` file, ships with repo) | SQL Server (SCM2, on-premise)  |
| ORM                        | EF Core (same code for both)         | EF Core                        |
| Stored Procs / Heavy Reads | Dapper                               | Dapper                         |
| Caching                    | Redis (Docker via Aspire)            | Redis (production instance)    |
| Auth to DB                 | N/A (SQLite)                         | SQL Server auth (user secrets) |

### Schema Tables

```
Users                  — AD-synced users, roles, theme preference
Customer               — e.g. US Army, US Navy, USAF
Product                — e.g. Gray Eagle, Predator B, Avenger
System                 — Customer + Product combination
RequestType            — System, Segment, CSCI (GA-ASI/Supplier), EE, TE, ThirdParty
RequestStatus          — Draft, Pending, In Review, Released, Rejected, Terminated
Request                — Core shared fields for all request types
RequestAttributes      — One row per request, type-specific JSON (CustomAttributes)
RequestSCMStatus       — SCM team status per request
RequestNotes           — Multiple notes per request (SVM Notes tab)
RequestAttachments     — File attachments per request
RequestHistory         — State transition log per request
RequestChangeLog       — Field-level change audit per request
Notifications          — In-app + email notification records
```

### Key Design Decisions

- `Request` table is the **SystemVersion** for System type requests
- `SystemId` is **nullable** — required for System/Segment/CSCI, NULL for EE/TE/ThirdParty
- `CustomAttributes` is a **JSON column** per request — shape varies by type
- `IsDeleted` + `LogDate` on **every table** — soft delete pattern
- `Users.Theme` stores the user's preferred Telerik theme
- `Users.Role` is a single column — one role per user (simple, not many-to-many)

### Hierarchy

```
Customer (Army) + Product (Gray Eagle)
    └── System (Army Gray Eagle)
            └── Request / SystemVersion (v4.2)
                    ├── Segment Request (Ground Control Segment)
                    │       └── CSCI GA-ASI (Flight Control Software)
                    │       └── CSCI Supplier (Engine Controller Firmware)
                    └── Segment Request (Comms Segment)
                            └── CSCI GA-ASI (HMI Portal)

EE Request     ← standalone, no SystemId
TE Request     ← standalone, no SystemId
Third Party SW ← standalone, no SystemId
```

### Seeded Demo Data

SQLite DB ships pre-seeded with:

- 3 Customers (Army, Navy, USAF)
- 3 Products (Gray Eagle, Predator B, Avenger)
- 3 Systems
- 5 Users (see Roles section)
- 6 Request Types + 8 Statuses
- Sample requests — one of each type with full attributes, notes, history

---

## 3. Security & Authentication

### Flow

```
User opens browser
    → Windows Auth proves domain identity (Kerberos — password never leaves DC)
        → SCM3 looks up AD username in Users table
            → Found: load Role, FullName, Department, Theme
            → Not found: auto-create user, assign Role = 'Viewer', notify SCM_Admin
                → Claims identity created from Users table data
                    → HTTP-only secure cookie issued
                        → All subsequent requests use cookie — no AD calls
```

### Key Principles

- **Password never touches application code. Ever.**
- Cookie is HTTP-only, Secure, SameSite=Strict
- Claims come from the `Users` table — not AD (after first login)
- Short cookie expiry + sliding window — idle users logged out automatically
- No JWT for current scope — Minimal API uses same Windows Auth + Cookie

### Cookie Claims

```csharp
new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
new Claim(ClaimTypes.Name,           user.FullName),
new Claim(ClaimTypes.Role,           user.Role),
new Claim("Department",              user.Department),
new Claim("Username",                user.Username)
```

### `ICurrentUserService`

Injected everywhere — gives instant access to the logged-in user's context:

```csharp
public interface ICurrentUserService
{
    User? CurrentUser { get; }
    string Role { get; }
    bool IsInRole(string role);
    bool CanEdit(Request request);
    bool CanApprove();
    bool CanRelease();
}
```

### Demo Login

Since this runs as a demo without a domain controller, a mock login page accepts:

| Username   | Password | Role      |
| ---------- | -------- | --------- |
| `user`     | `test`   | Viewer    |
| `scm`      | `test`   | SCM_Staff |
| `scmadmin` | `test`   | SCM_Admin |
| `teuser`   | `test`   | TE_User   |
| `eeuser`   | `test`   | EE_User   |

---

## 4. Roles & Permissions

### Role Definitions

| Role        | Who                    | Description                      |
| ----------- | ---------------------- | -------------------------------- |
| `Viewer`    | Read-only stakeholders | View all requests, read-only     |
| `TE_User`   | Test engineers         | Create and view TE Requests only |
| `EE_User`   | Electrical engineers   | Create and view EE Requests only |
| `SCM_Staff` | SCM team members       | All requests + SCM DB Manager    |
| `SCM_Admin` | SCM leads, you         | Everything + Admin menu          |

### Permission Matrix

| Action            | Viewer | TE_User   | EE_User   | SCM_Staff | SCM_Admin |
| ----------------- | ------ | --------- | --------- | --------- | --------- |
| View all requests | ✅     | Own only  | Own only  | ✅        | ✅        |
| Create request    | ❌     | TE only   | EE only   | ✅        | ✅        |
| Edit request      | ❌     | Own+Draft | Own+Draft | ✅        | ✅        |
| Delete (soft)     | ❌     | ❌        | ❌        | ❌        | ✅        |
| Update SCM Status | ❌     | ❌        | ❌        | ✅        | ✅        |
| Add notes         | ❌     | Own only  | Own only  | ✅        | ✅        |
| Approve / Reject  | ❌     | ❌        | ❌        | ❌        | ✅        |
| Release           | ❌     | ❌        | ❌        | ❌        | ✅        |
| Reopen            | ❌     | ❌        | ❌        | ❌        | ✅        |
| View History      | ✅     | Own only  | Own only  | ✅        | ✅        |
| View Change Log   | ❌     | ❌        | ❌        | ✅        | ✅        |
| SCM DB Manager    | ❌     | ❌        | ❌        | ✅        | ✅        |
| Admin menu        | ❌     | ❌        | ❌        | ❌        | ✅        |
| Manage Users      | ❌     | ❌        | ❌        | ❌        | ✅        |

### Three Layers of Enforcement

```csharp
// 1. Route level — page won't load
@attribute [Authorize(Roles = "SCM_Admin,SCM_Staff")]

// 2. UI level — button/tab hidden
@if (CurrentUser.CanRelease()) { <TelerikButton>Release</TelerikButton> }

// 3. Service level — last line of defense
if (!currentUser.CanEdit(request))
    throw new UnauthorizedAccessException();
```

---

## 5. Request Types

### Type Overview

| Type           | Hierarchy        | Standalone | Created By                    |
| -------------- | ---------------- | ---------- | ----------------------------- |
| System         | ✅ top level     |            | SCM_Staff, SCM_Admin          |
| Segment        | ✅ under System  |            | SCM_Staff, SCM_Admin          |
| CSCI GA-ASI    | ✅ under Segment |            | SCM_Staff, SCM_Admin          |
| CSCI Supplier  | ✅ under Segment |            | SCM_Staff, SCM_Admin          |
| EE Request     |                  | ✅         | EE_User, SCM_Staff, SCM_Admin |
| TE Request     |                  | ✅         | TE_User, SCM_Staff, SCM_Admin |
| Third Party SW |                  | ✅         | SCM_Staff, SCM_Admin          |

### Per-Type UI Field Reference

> **Target shape for the upcoming detail-panel redesign** (LHS-feedback session,
> 2026-06-07 — supersedes the old `CustomAttributes` JSON-shape examples that used to
> live here). The seeded `CustomAttributes` JSON in `SCM3.Data/Seed/SCM3Seeder.cs` still
> reflects the smaller, earlier shape and will need to be reconciled with this reference
> when the redesign is implemented — that reconciliation is its own future task, not
> part of this doc update.

Each artifact type's detail panel renders the same five top-level accordion sections —
**Release Information, Relationships, Attachments, SCM Status, History** — populated with
type-specific fields/groups (see "UI Section Structure" below for how these map to the
per-type breakdowns). Field lists are transcribed directly from the UI spec, including
type-specific naming quirks (e.g. "LRE Part Numbers" for Supplier CSCI, distinct from
"LRU Part Numbers" elsewhere — not a typo to "fix", just what that type calls it).

**System Release:**
- *Release Information* — Request ID · Customer · Product · Name · Version · Parent
  Version · Priority · Submission Date · Ready Date · Need Date · Release Date ·
  Release Description · Notes To SCM
  - *Requestor Information* — Requestor Name · Requestor Email · Requestor Phone ·
    Charge Number
- *Associations*
  - *Segments* — Segment Name · Version · State
- *Associated Items*
  - *Segments* — Segment Name · Version
  - *Compatible Software* — Name · Version
- *Attachments* — SVD · SVM · Supporting Documents
- *SCM Status* — Assigned To · Expected Completion Date · On Hold · Work Description ·
  SCM Notes · Reviewer Notes · Comments · Release Readiness Notes
- *History* — State Changes · Comments · Notes

**Segment Release:**
- *Release Information* — Request ID · Name · Version · Parent System · Submission Date ·
  Need Date · Release Date · Release Description · Notes To SCM
- *Associations*
  - *CSCIs* — CSCI Name · Version · State
- *Attachments* — Supporting Documents
- *SCM Status* — Assigned To · Expected Completion Date · On Hold · Work Description ·
  SCM Notes · Reviewer Notes · Comments
- *History* — State Changes · Comments · Notes

**ASI CSCI Release:**
- *Release Information* — Request ID · Submission Date · Release Date · Name · Version ·
  Parent Version · Release Description · Notes To SCM · CSCI Only Release (Boolean)
  - *DCNs* — DCN Number · Title · Status
- *Associated Items*
  - *Common Libraries* — Library Name · Version
  - *Build Targets* — Build Target · Platform
  - *LRU Part Numbers* — Part Number · Description
  - *Associated Third Party Software* — Vendor · Product · Version
- *Attachments* — Release Notes · Checksums · Build Manifest · Supporting Documents
- *SCM Status* — Assigned To · Expected Completion Date · On Hold · Work Description ·
  SCM Notes · Reviewer Notes · Comments
- *History* — State Changes · Comments · Notes

**Supplier CSCI Release:**
- *Release Information* — Request ID · Submission Date · Release Date · Name · Version ·
  Software Point Of Contact · Supplier · Supplier Point Of Contact · Supplier POC Phone ·
  Supplier POC Email · Location · Release Description · Notes To SCM
  - *DCNs* — DCN Number · Title · Status
- *Associated Items*
  - *Binary Checksums* — Checksum · Algorithm
  - *LRE Part Numbers* — Part Number · Description
- *Attachments* — Supplier Release Package · Checksums · Supporting Documents
- *SCM Status* — Assigned To · Expected Completion Date · On Hold · Work Description ·
  SCM Notes · Reviewer Notes · Comments
- *History* — State Changes · Comments · Notes

**Third Party Software Request:**
- *Product Information* — Vendor · Product Name · Product Version · Country Of Origin ·
  Source · Product URL · Source Location · Internal Location · Download File Names ·
  Download Location · Intended Use · Platform · Point Of Contact · Binary / Source / Both
- *Licensing* — License Type · Product License · Contains OSS · Patent · Patent Non
  Assert Needed · Patent Non Assert Approval · Software License Agreement · License URL ·
  License Tracking Method · OSS Obligations · Export Compliance · Manager Approval ·
  Approver · Approval Date · Status
- *Attachments* — Licensing Documents · Approval Documents · Supporting Documents
- *SCM Status* — Assigned To · Expected Completion Date · On Hold · Work Description ·
  SCM Notes · Reviewer Notes · Comments
- *History* — State Changes · Comments · Notes

> Note: for this type, *Product Information* + *Licensing* take the place of
> *Release Information* — Third Party SW isn't a "release" in the same sense as the
> others, so it has no Relationships section either.

**Electrical Engineering Release:**
- *Release Information* — Request ID · Part Number · Assembly Part Number · Assembly
  Description · Data Rights · Change Notice · Release Location · Charge Number ·
  Submission Date · Need Date · Release Date · Release Description · Notes To SCM
- *Attachments* — Drawings · Supporting Documents
- *SCM Status* — Assigned To · Expected Completion Date · On Hold · Work Description ·
  SCM Notes · Reviewer Notes · Comments
- *History* — State Changes · Comments · Notes

> **TE Request** wasn't part of this new field spec — its existing `CustomAttributes`
> shape (Summary / ContactPerson / Email / Phone / WorkOrder / RackComputer / Operation /
> Type, dropdown values `Proven Report` | `PTP Based`) stands as-is and remains TBD for
> this redesign (Open Item §17.1).

### Hierarchy

```
System
└── Segment
    └── CSCI
```

- A System contains Segments.
  - A Segment can also be associated with other Systems, but is unique to (owned by)
    this System.
- A Segment contains CSCIs.
- A CSCI belongs to one Segment.
  - A CSCI can also be associated with other Segments, but is unique to (owned by)
    this Segment.
- Supplier CSCI, Third Party Software, and EE requests are independent artifact types —
  they don't sit in the System/Segment/CSCI hierarchy, but use the same portal shell
  and workflow as everything else.

### UI Section Structure

All artifact types render the same five top-level sections as **accordions** (not tabs —
see root CLAUDE.md §7 for the broader detail-panel redesign): **Release Information,
Relationships, Attachments, SCM Status, Audit Log**. "Relationships" is the umbrella for
the per-type *Associations* / *Associated Items* groups in the field reference above —
its content (and whether it appears at all — Third Party SW has none) varies by artifact
type, while Attachments / SCM Status / Audit Log keep the same shape everywhere, just
with different named slots/fields.

---

## 6. Request Workflow

### State Machine

```
[Draft]
    ↓ Requestor clicks Submit
[Pending]
    ↓ SCM picks up          ↘ SCM_Admin Terminates
[In Review]
    ↓ SCM_Admin Approves    ↘ SCM_Admin/CCB Rejects    ↘ SCM_Admin Terminates
[Released]
    ↓ SCM_Admin Reopens
[In Review]  ← re-enters the flow
```

### Valid Transitions

| From      | To         | Action    | Who                             |
| --------- | ---------- | --------- | ------------------------------- |
| Draft     | Pending    | Submit    | Requestor, SCM_Staff, SCM_Admin |
| Pending   | In Review  | Pick Up   | SCM_Staff, SCM_Admin            |
| Pending   | Terminated | Terminate | SCM_Admin                       |
| In Review | Released   | Release   | SCM_Admin                       |
| In Review | Rejected   | Reject    | SCM_Admin, CCB_Member           |
| In Review | Terminated | Terminate | SCM_Admin                       |
| Released  | In Review  | Reopen    | SCM_Admin                       |

### Actions Bar (role-driven visibility)

| Button            | Visible To                  | Available When     |
| ----------------- | --------------------------- | ------------------ |
| Save              | Owner, SCM_Staff, SCM_Admin | Draft, In Review   |
| Submit            | Owner, SCM_Staff, SCM_Admin | Draft              |
| Request More Info | SCM_Staff, SCM_Admin        | Pending, In Review |
| Release           | SCM_Admin                   | In Review          |
| Reject            | SCM_Admin                   | In Review          |
| Terminate         | SCM_Admin                   | Pending, In Review |
| Reopen            | SCM_Admin                   | Released           |

---

## 7. UI / UX

### Overall Layout

```
┌─────────────────────────────────────────────────────────┐
│  [≡] SCM3 Portal                          🔔  Ashish D  │  ← Header
├──────────┬──────────────────────────────────────────────┤
│          │  [Header Strip]                               │
│  Left    │  Title · Type · ID · Parent · Requestor Info  │
│  Nav     ├──────────────────────────────────────────────┤
│  Drawer  │  [Actions Bar]  Save Submit Release ...  🕐 📋│
│          ├──────────────────────────────────────────────┤
│  200px   │  Release Info | Segments | Assoc. | Attach.. │  ← Tabs
│  expand  │ ─────────────────────────────────────────── │
│  50px    │  [Accordion Section 1]              ▼        │
│  collapse│  [Accordion Section 2]              ▼        │
│          │                                               │
├──────────┴──────────────────────────────────────────────┤
```

### Master-Detail (Left Panel)

- Filter chips: All · In Review · Pending · Released · Draft
- Global search bar — searches Name, ID, Requestor, Version, Parent
- Export Excel button (Telerik built-in grid export)
- Each list item shows:
  ```
  [Type Icon]  Request Name v4.2          [In Review]
               System · ID 3001 · Parent — · Requestor John Smith
               2026-05-25                              💬 4
  ```
- Clicking an item loads detail panel on the right — **no page navigation**

### Detail Page — Header Strip

```
UAS Mission System v4.0
System · ID 3001 · Version v4.0
─────────────────────────────────────────────────────────
REQUESTOR        EMAIL / PHONE              CHARGE #    DATE
John Smith       john@ga-asi.com            CN-2026-001 2026-05-25
                 (858) 555-1234
─────────────────────────────────────────────────────────
STATUS: [In Review]    PRIORITY: High
```

### Tab Strip

```
Release Information | Segments | Associated Items | Attachments | SCM Status | SVM Notes
```

- Each tab contains **collapsible accordion sections**
- **Inline editing** — fields editable directly, no Edit button needed
- **SVM Notes tab** — text box + Add button, newest on top, always accessible to SCM

### Slide-In Panels (RHS Drawer)

| Panel      | Trigger                  | Roles                | Content                       |
| ---------- | ------------------------ | -------------------- | ----------------------------- |
| History    | 🕐 button in actions bar | All                  | Timeline of state transitions |
| Change Log | 📋 button in actions bar | SCM_Staff, SCM_Admin | Table of field-level changes  |

### Expand / Collapse Controls

```
[Expand All]  [Collapse All]  [Show / Hide Sections]
```

Per the screenshot — these appear above the accordion sections on the detail panel.

---

## 8. Navigation

### Left Collapsible Drawer

Expanded: **200px** · Collapsed: **50px** (icons only)
Items only appear if the user has access — no grayed-out items.

```
📋 Release Artifacts
    ├── All Requests          ← All roles
    ├── System Requests       ← All roles
    ├── Segment Requests      ← All roles
    ├── CSCI Requests         ← All roles
    ├── EE Requests           ← EE_User, SCM_Staff, SCM_Admin
    ├── TE Requests           ← TE_User, SCM_Staff, SCM_Admin
    └── Third Party SW        ← SCM_Staff, SCM_Admin

🔧 Viewer Tools
    ├── Lineage Tree          ← All roles
    ├── Release Documents     ← All roles
    ├── Release Tools         ← SCM_Staff, SCM_Admin
    └── Compare Tools         ← SCM_Staff, SCM_Admin

🗄️ SCM DB Manager            ← SCM_Staff, SCM_Admin
    ├── Customers
    ├── Products
    ├── Systems
    ├── CSCI Names
    ├── Segment Names
    ├── Manufacturers
    └── Units

⚙️ Admin                     ← SCM_Admin only
    ├── Manage Users
    ├── Roles & Permissions
    └── Settings
```

> Note: SCM DB Manager manages reference data for the entire SCM Portal ecosystem,
> not just Release Artifacts. Manufacturers and Units support other tools and services.

---

## 9. Services

### Interface Layer (`SCM3.Web/Services/`)

```
IRequestService.cs         — CRUD + filtering for requests
ISystemService.cs          — System hierarchy queries
ICustomerService.cs        — Customer lookups
IProductService.cs         — Product lookups
ICurrentUserService.cs     — Logged-in user context, role checks
CurrentUserService.cs      — Implementation (reads from cookie claims)
CacheService.cs            — Redis wrapper for lookup caching
INotificationService.cs    — In-app notifications
IEmailService.cs           — HTML email via SMTP
```

### Implementation Layer (`SCM3.Data/Services/`)

```
RequestService.cs          — EF Core + Dapper implementation
SystemService.cs
CustomerService.cs
ProductService.cs
NotificationService.cs
```

### API Layer (`SCM3.Api/Endpoints/`)

```
RequestEndpoints.cs
SystemEndpoints.cs
UserEndpoints.cs
```

### Cached Lookups (Redis)

The following rarely-changing data is cached in Redis:

- `RequestType` list
- `RequestStatus` list
- `Customer` list
- `Product` list
- Dashboard stats (refreshed every 5 minutes)

Frequently changing data (requests, notes, history) always goes straight to DB.

---

## 10. Notifications & Email

### Every Action Triggers

1. **In-app notification** — bell icon, real-time via SignalR
2. **HTML email** — via internal GA-ASI SMTP server (MailKit)
3. **Notification record** in DB — confirms email sent successfully
4. **History entry** — every notification logged as a history entry

### Notification Triggers

| Action            | Who Gets Notified        |
| ----------------- | ------------------------ |
| Request submitted | SCM_Staff, SCM_Admin     |
| Request assigned  | Assigned SCM user        |
| Status changed    | Requestor + SCM team     |
| Note added        | Requestor + assigned SCM |
| Approved          | Requestor                |
| Rejected          | Requestor                |
| Released          | Requestor + CCB          |
| Request More Info | Requestor                |
| Terminated        | Requestor                |
| Reopened          | Requestor + SCM team     |

### Email Configuration

```json
// appsettings.json (non-sensitive)
"Smtp": {
    "Port": 587,
    "FromAddress": "scm3-portal@ga-asi.com",
    "FromName": "SCM3 Portal"
}

// user-secrets (sensitive)
"Smtp:Host": "mail.ga-asi.internal",
"Smtp:Username": "...",
"Smtp:Password": "..."
```

### Email Templates

- Razor-based HTML templates — one per action type
- GA-ASI branded (matches portal theme)
- Plain text fallback included
- Located in `SCM3.Web/EmailTemplates/`

---

## 11. Validation

### FluentValidation

- One validator class per request type
- Conditional rules — e.g. Supplier fields only required for CSCI Supplier type
- Inline field errors shown immediately
- Tab-level error badges — red dot on tab if that tab has validation errors
- Runs client-side via Blazor + server-side before save

```csharp
// Example: CSCI Supplier validator
RuleFor(x => x.SupplierName)
    .NotEmpty()
    .When(x => x.RequestTypeId == RequestTypes.CsciSupplier);
```

---

## 12. Real-Time (SignalR)

### What Gets Updated in Real-Time

| Event             | Who Sees It                    |
| ----------------- | ------------------------------ |
| Status change     | All users viewing that request |
| Note added        | All users viewing that request |
| SCM Status update | All users viewing that request |
| Notification bell | The targeted user              |
| Dashboard stats   | All users on dashboard         |

### Redis as SignalR Backplane

Required when running multiple server instances.
Aspire wires this up automatically:

```csharp
// SCM3.AppHost/Program.cs
var redis = builder.AddRedis("cache");
builder.AddProject<SCM3_Web>().WithReference(redis);
```

---

## 13. Themes

### Strategy

- **Telerik ThemeBuilder** — custom GA-ASI theme created at themebuilder.telerik.com
- GA-ASI theme + dark variant as the default options
- Users can switch theme and preference is saved to `Users.Theme` column
- Dynamic theme loading in `App.razor` — no page reload needed

### Available Themes

| Theme            | File                 | Description              |
| ---------------- | -------------------- | ------------------------ |
| `scm3-gasi`      | `scm3-gasi.css`      | GA-ASI branded (default) |
| `scm3-gasi-dark` | `scm3-gasi-dark.css` | GA-ASI dark mode         |
| `default`        | Telerik CDN          | Telerik default          |
| `fluent`         | Telerik CDN          | Microsoft Fluent style   |
| `bootstrap`      | Telerik CDN          | Bootstrap style          |

### File Structure

```
SCM3.Web/wwwroot/
    css/
        themes/
            scm3-gasi.css          ← custom GA-ASI theme (ThemeBuilder export)
            scm3-gasi-dark.css     ← dark variant
        scm3.css                   ← layout overrides only (~150 lines)
        scm3.variables.css         ← CSS custom properties for GA-ASI branding
    js/
        scm3.js                    ← minimal JS helpers
```

### CSS Custom Properties

```css
:root {
  --scm3-primary: #1a5276;
  --scm3-accent: #2e86c1;
  --scm3-sidebar-bg: #1c2833;
  --scm3-header-bg: #ffffff;
  --scm3-expanded-w: 200px;
  --scm3-collapsed-w: 50px;
}
```

### Production Optimization

- Minification: automatic via `dotnet publish`
- Compression: Brotli + Gzip via `app.UseResponseCompression()`
- Cache busting: static asset fingerprinting built into .NET 10
- Telerik CSS: already minified in NuGet package

---

## 14. Logging

### Serilog Configuration

Configured once in `SCM3.ServiceDefaults/Extensions.cs`, bootstrapped in `SCM3.Web/Program.cs`.

### Sinks

| Sink           | Environment | Purpose                            |
| -------------- | ----------- | ---------------------------------- |
| Console        | All         | Aspire dashboard picks this up     |
| File           | All         | Rolling daily log files in `logs/` |
| Seq (optional) | Dev         | Structured log viewer              |

### Packages

```
Serilog.AspNetCore
Serilog.Sinks.Console
Serilog.Sinks.File
Serilog.Enrichers.Thread
Serilog.Enrichers.Environment
Serilog.Settings.Configuration
```

---

## 15. CLAUDE.md Files

Each project has its own `CLAUDE.md` giving Claude Code instant orientation:

| File                     | Content                                                     |
| ------------------------ | ----------------------------------------------------------- |
| `CLAUDE.md` (root)       | This document — solution overview, decisions, entity recipe |
| `SCM3.Web/CLAUDE.md`     | Blazor patterns, Telerik components, tabs, roles, nav       |
| `SCM3.Data/CLAUDE.md`    | EF Core, repositories, SQLite→SQL Server swap guide         |
| `SCM3.AppHost/CLAUDE.md` | Aspire wiring, Redis, SignalR, how to add services          |
| `SCM3.Api/CLAUDE.md`     | Minimal API, endpoint conventions                           |

---

## 16. New Domain Entity Recipe

> Follow these steps whenever adding a new domain entity (e.g. CSCI Name, Manufacturer).
> Uses **CSCI Name** as the example throughout.

### Step 1 — Add the Entity (`SCM3.Data/Entities/`)

```csharp
public class CsciName
{
    public int CsciNameId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime LogDate { get; set; } = DateTime.UtcNow;
}
```

### Step 2 — Add to DbContext (`SCM3.Data/DbContext/SCM3DbContext.cs`)

```csharp
public DbSet<CsciName> CsciNames { get; set; }
```

### Step 3 — Add Migration

```bash
dotnet ef migrations add AddCsciName --project SCM3.Data
dotnet ef database update --project SCM3.Data
```

### Step 4 — Add Seed Data (`SCM3.Data/Seed/SCM3Seeder.cs`)

```csharp
if (!context.CsciNames.Any())
{
    context.CsciNames.AddRange(
        new CsciName { Name = "Flight Control Software" },
        new CsciName { Name = "HMI Portal" }
    );
    await context.SaveChangesAsync();
}
```

### Step 5 — Add Repository (`SCM3.Data/Repositories/`)

```csharp
public interface ICsciNameRepository
{
    Task<List<CsciName>> GetAllAsync();
    Task<CsciName?> GetByIdAsync(int id);
    Task AddAsync(CsciName entity);
    Task UpdateAsync(CsciName entity);
    Task DeleteAsync(int id);  // soft delete — sets IsDeleted = true
}
```

### Step 6 — Add Service Interface (`SCM3.Web/Services/`)

```csharp
public interface ICsciNameService
{
    Task<List<CsciName>> GetAllAsync();
    Task<CsciName?> GetByIdAsync(int id);
    Task SaveAsync(CsciName entity);
    Task DeleteAsync(int id);
}
```

### Step 7 — Implement Service (`SCM3.Data/Services/`)

```csharp
public class CsciNameService : ICsciNameService
{
    private readonly ICsciNameRepository _repo;
    private readonly CacheService _cache;
    // inject and implement...
}
```

### Step 8 — Register in DI (`SCM3.Web/Program.cs`)

```csharp
builder.Services.AddScoped<ICsciNameService, CsciNameService>();
```

### Step 9 — Add Blazor Page (`SCM3.Web/Components/Pages/Admin/`)

```
CsciNames.razor         ← list + inline edit (Telerik Grid)
```

### Step 10 — Add to Navigation (`SCM3.Web/Components/Layout/MainLayout.razor`)

```csharp
// Under SCM DB Manager section, role-filtered
@if (currentUser.IsInRole("SCM_Staff", "SCM_Admin"))
{
    <NavItem Href="/admin/csci-names" Icon="@SvgIcon.Code" Label="CSCI Names" />
}
```

---

## 17. Open Items / TBD

| #   | Item                                   | Notes                                                                                                                              |
| --- | -------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------- |
| 1   | TE Request — additional fields         | Summary, Contact, Email, Phone, Work Order, Rack Computer, Operation, Type (Proven Report / PTP Based) confirmed. More fields TBD. |
| 2   | SCM DB Manager — Manufacturers + Units | Full field definitions TBD. These support other SCM tools beyond Release Artifacts.                                                |
| 3   | GA-ASI SMTP server address             | Goes in user-secrets, never committed to Git                                                                                       |
| 4   | Telerik license key setup              | Each dev sets via user-secrets or environment variable on office machines                                                          |
| 5   | GA-ASI brand colors                    | Needed for ThemeBuilder custom theme creation                                                                                      |
| 6   | GitLab repo name + internal URL        | TBD when repo is created                                                                                                           |
| 7   | CCB role                               | CCB members mentioned in workflow — may need a 6th role                                                                            |
| 8   | Lineage Tree                           | Tool listed in nav — full requirements TBD                                                                                         |
| 9   | Release Tools                          | Tool listed in nav — full requirements TBD                                                                                         |
| 10  | Compare Tools                          | Tool listed in nav — full requirements TBD                                                                                         |
| 11  | Release Documents                      | Tool listed in nav — full requirements TBD                                                                                         |

---

## 18. Testing

### Projects

| Project                  | Type                     | Purpose                                 |
| ------------------------ | ------------------------ | --------------------------------------- |
| `SCM3.Tests.Unit`        | xUnit + Moq              | Services, validators, business logic    |
| `SCM3.Tests.Integration` | xUnit + EF Core InMemory | Repository + DB tests, API endpoints    |
| `SCM3.Tests.UI`          | Playwright               | End-to-end Blazor UI tests (v2 backlog) |

### Packages

| Package                                  | Purpose                            |
| ---------------------------------------- | ---------------------------------- |
| `xUnit`                                  | Test framework                     |
| `xUnit.runner.visualstudio`              | VS Code test runner integration    |
| `Moq`                                    | Mocking interfaces                 |
| `FluentAssertions`                       | Readable assertions                |
| `Microsoft.EntityFrameworkCore.InMemory` | In-memory DB for integration tests |
| `coverlet.collector`                     | Code coverage                      |
| `Playwright`                             | UI testing (v2)                    |

### What Gets Tested

| Layer                  | Test Type   | Examples                                           |
| ---------------------- | ----------- | -------------------------------------------------- |
| FluentValidation rules | Unit        | CSCI Supplier fields required when type = Supplier |
| Service logic          | Unit        | Workflow transitions, permission checks            |
| Repository             | Integration | EF Core queries against InMemory DB                |
| Notification triggers  | Unit        | Correct users notified per action                  |
| API endpoints          | Integration | Minimal API endpoint response codes and payloads   |

### Running Tests

```bash
# All tests
dotnet test

# Unit only
dotnet test SCM3.Tests.Unit

# Integration only
dotnet test SCM3.Tests.Integration

# With coverage
dotnet test --collect:"XPlat Code Coverage"
```
