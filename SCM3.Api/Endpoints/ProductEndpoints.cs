using SCM3.Data.Entities;
using SCM3.Data.Services;

namespace SCM3.Api.Endpoints;

// Product reference data (root CLAUDE.md §9) — managed under SCM DB Manager and
// used when composing Systems.
public static class ProductEndpoints
{
    public static RouteGroupBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products").WithTags("Products");

        group.MapGet("/", async (IProductService service)
            => Results.Ok(await service.GetAllAsync()));

        group.MapGet("/{id:int}", async (IProductService service, int id)
            => await service.GetByIdAsync(id) is { } product ? Results.Ok(product) : Results.NotFound());

        group.MapPost("/", async (IProductService service, Product product) =>
        {
            await service.SaveAsync(product);
            return Results.Created($"/api/products/{product.ProductId}", product);
        });

        group.MapPut("/{id:int}", async (IProductService service, int id, Product product) =>
        {
            product.ProductId = id;
            await service.SaveAsync(product);
            return Results.NoContent();
        });

        group.MapDelete("/{id:int}", async (IProductService service, int id) =>
        {
            await service.DeleteAsync(id);
            return Results.NoContent();
        });

        return group;
    }
}
