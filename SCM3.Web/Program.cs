using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using SCM3.Web.Components;
using SCM3.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// --- SCM3.Api client — Web -> Api -> Data -> DB (root CLAUDE.md architecture pivot:
// SCM3.Api is the ONLY project that talks to the database; Web never registers
// SCM3DbContext, repositories, or SCM3.Data.Services directly). The interfaces below
// are thin typed-HttpClient wrappers around SCM3.Api's MapGroup endpoints — "https+http"
// is an Aspire service-discovery URI resolved once SCM3.AppHost wires Web and Api
// together (AddServiceDefaults already registers AddServiceDiscovery on every client).
var apiBaseUrl = builder.Configuration["Api:BaseUrl"] ?? "https+http://api";

builder.Services.AddHttpClient<IRequestService, RequestService>(client => client.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<ISystemService, SystemService>(client => client.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<ICustomerService, CustomerService>(client => client.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<IProductService, ProductService>(client => client.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<INotificationService, NotificationService>(client => client.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<IUserService, UserService>(client => client.BaseAddress = new Uri(apiBaseUrl));

// --- Web-layer services (cookie claims, Redis, SMTP — root CLAUDE.md §9) ---------------
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<CacheService>();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("cache") ?? "localhost:6379";
    options.InstanceName = "scm3:";
});

// --- Authentication — Windows Auth proves identity, then claims from the Users table
// ride an HTTP-only, Secure, SameSite=Strict cookie (root CLAUDE.md §3). The demo mock
// login page below issues this same cookie for the 5 demo accounts. --------------------
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/access-denied";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
    });
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddTelerikBlazor();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

app.MapDefaultEndpoints();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Demo login endpoint (root CLAUDE.md §3) — sign-in must happen on a regular HTTP
// request (writes to the response), which Blazor Server's interactive circuit can't do
// mid-render, hence this lives here rather than inside a component. Login.razor posts a
// plain HTML form (with its own antiforgery token) here; [FromForm] binds the fields.
// The user lookup goes through SCM3.Api (Web -> Api -> Data -> DB) — Web has no DbContext.
app.MapPost("/login", async (HttpContext http, IUserService userService,
    [FromForm] string username, [FromForm] string password, [FromForm] string? returnUrl) =>
{
    if (password != "test")
    {
        return Results.Redirect("/login?error=1");
    }

    var user = await userService.GetByUsernameAsync(username);
    if (user is null)
    {
        return Results.Redirect("/login?error=1");
    }

    var claims = new List<System.Security.Claims.Claim>
    {
        new(System.Security.Claims.ClaimTypes.NameIdentifier, user.UserId.ToString()),
        new(System.Security.Claims.ClaimTypes.Name, user.FullName),
        new(System.Security.Claims.ClaimTypes.Role, user.Role),
        new("Department", user.Department ?? string.Empty),
        new("Username", user.Username)
    };

    var principal = new System.Security.Claims.ClaimsPrincipal(
        new System.Security.Claims.ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));

    await http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

    return Results.Redirect(string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl);
});

app.MapPost("/logout", async (HttpContext http) =>
{
    await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/login");
});

app.Run();
