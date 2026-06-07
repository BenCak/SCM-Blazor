using SCM3.Data.Entities;
using SCM3.Data.Services;

namespace SCM3.Api.Endpoints;

// Customer reference data (root CLAUDE.md §9) — managed under SCM DB Manager and
// used when composing Systems.
public static class CustomerEndpoints
{
    public static RouteGroupBuilder MapCustomerEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/customers").WithTags("Customers");

        group.MapGet("/", async (ICustomerService service)
            => Results.Ok(await service.GetAllAsync()));

        group.MapGet("/{id:int}", async (ICustomerService service, int id)
            => await service.GetByIdAsync(id) is { } customer ? Results.Ok(customer) : Results.NotFound());

        group.MapPost("/", async (ICustomerService service, Customer customer) =>
        {
            await service.SaveAsync(customer);
            return Results.Created($"/api/customers/{customer.CustomerId}", customer);
        });

        group.MapPut("/{id:int}", async (ICustomerService service, int id, Customer customer) =>
        {
            customer.CustomerId = id;
            await service.SaveAsync(customer);
            return Results.NoContent();
        });

        group.MapDelete("/{id:int}", async (ICustomerService service, int id) =>
        {
            await service.DeleteAsync(id);
            return Results.NoContent();
        });

        return group;
    }
}
