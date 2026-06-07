using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using SCM3.Api.Endpoints;
using SCM3.Data.DbContext;
using SCM3.Data.Repositories;
using SCM3.Data.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// SCM3.Api is the only project that talks to the database — SCM3.Web reaches it over
// HTTP via the MapGroup endpoints below (Web -> Api -> Data -> DB). Same provider switch
// as SCM3.Web (root CLAUDE.md §1): one config flag decides SQLite (demo) vs SQL Server.
var dataProvider = builder.Configuration["DataProvider"] ?? "Sqlite";
var connectionString = builder.Configuration.GetConnectionString("SCM3") ?? "Data Source=../SCM3.Data/scm3.db";

builder.Services.AddDbContext<SCM3DbContext>(options =>
{
    if (string.Equals(dataProvider, "SqlServer", StringComparison.OrdinalIgnoreCase))
    {
        options.UseSqlServer(connectionString);
    }
    else
    {
        options.UseSqlite(connectionString);
    }
});

builder.Services.AddScoped<IRequestRepository, RequestRepository>();
builder.Services.AddScoped<ISystemRepository, SystemRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<IRequestService, RequestService>();
builder.Services.AddScoped<ISystemService, SystemService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IUserService, UserService>();

// Entities carry EF navigation properties that form cycles (Request <-> RequestHistory
// <-> RequestStatus, etc.) — ignore rather than throw when serializing them as JSON.
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.MapRequestEndpoints();
app.MapSystemEndpoints();
app.MapCustomerEndpoints();
app.MapProductEndpoints();
app.MapNotificationEndpoints();
app.MapUserEndpoints();

app.Run();
