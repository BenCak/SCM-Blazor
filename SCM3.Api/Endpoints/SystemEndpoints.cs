using SCM3.Data.Entities;
using SCM3.Data.Services;

namespace SCM3.Api.Endpoints;

// System hierarchy queries (root CLAUDE.md §9) — Customer + Product -> System ->
// Request/SystemVersion -> Segment -> CSCI (root CLAUDE.md §2).
public static class SystemEndpoints
{
    public static RouteGroupBuilder MapSystemEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/systems").WithTags("Systems");

        group.MapGet("/", async (ISystemService service)
            => Results.Ok(await service.GetAllAsync()));

        group.MapGet("/{id:int}", async (ISystemService service, int id)
            => await service.GetByIdAsync(id) is { } system ? Results.Ok(system) : Results.NotFound());

        group.MapGet("/by-customer/{customerId:int}", async (ISystemService service, int customerId)
            => Results.Ok(await service.GetByCustomerAsync(customerId)));

        group.MapGet("/by-product/{productId:int}", async (ISystemService service, int productId)
            => Results.Ok(await service.GetByProductAsync(productId)));

        group.MapPost("/", async (ISystemService service, SystemEntity system) =>
        {
            await service.SaveAsync(system);
            return Results.Created($"/api/systems/{system.SystemId}", system);
        });

        group.MapPut("/{id:int}", async (ISystemService service, int id, SystemEntity system) =>
        {
            system.SystemId = id;
            await service.SaveAsync(system);
            return Results.NoContent();
        });

        group.MapDelete("/{id:int}", async (ISystemService service, int id) =>
        {
            await service.DeleteAsync(id);
            return Results.NoContent();
        });

        return group;
    }
}
