using SCM3.Data.Entities;
using SCM3.Data.Services;

namespace SCM3.Api.Endpoints;

// Backs the demo login lookup, ICurrentUserService's claims resolution, and
// Admin > Manage Users (root CLAUDE.md §3, §8).
public static class UserEndpoints
{
    public static RouteGroupBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users").WithTags("Users");

        group.MapGet("/", async (IUserService service)
            => Results.Ok(await service.GetAllAsync()));

        group.MapGet("/{id:int}", async (IUserService service, int id)
            => await service.GetByIdAsync(id) is { } user ? Results.Ok(user) : Results.NotFound());

        group.MapGet("/by-username/{username}", async (IUserService service, string username)
            => await service.GetByUsernameAsync(username) is { } user ? Results.Ok(user) : Results.NotFound());

        group.MapPost("/", async (IUserService service, User user) =>
        {
            await service.SaveAsync(user);
            return Results.Created($"/api/users/{user.UserId}", user);
        });

        group.MapPut("/{id:int}", async (IUserService service, int id, User user) =>
        {
            user.UserId = id;
            await service.SaveAsync(user);
            return Results.NoContent();
        });

        group.MapDelete("/{id:int}", async (IUserService service, int id) =>
        {
            await service.DeleteAsync(id);
            return Results.NoContent();
        });

        return group;
    }
}
