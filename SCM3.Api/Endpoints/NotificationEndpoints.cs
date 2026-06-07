using SCM3.Data.Services;

namespace SCM3.Api.Endpoints;

// In-app notification feed (root CLAUDE.md §9, §10) — bell-icon list, unread count,
// and read-state updates. The triggering side (NotifyAsync) is invoked from the
// service layer when actions occur, not exposed directly here.
public static class NotificationEndpoints
{
    public static RouteGroupBuilder MapNotificationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/notifications").WithTags("Notifications");

        group.MapGet("/user/{recipientUserId:int}", async (INotificationService service, int recipientUserId, bool? unreadOnly)
            => Results.Ok(await service.GetForUserAsync(recipientUserId, unreadOnly ?? false)));

        group.MapGet("/user/{recipientUserId:int}/unread-count", async (INotificationService service, int recipientUserId)
            => Results.Ok(await service.GetUnreadCountAsync(recipientUserId)));

        group.MapPost("/{id:int}/read", async (INotificationService service, int id) =>
        {
            await service.MarkAsReadAsync(id);
            return Results.NoContent();
        });

        group.MapPost("/user/{recipientUserId:int}/read-all", async (INotificationService service, int recipientUserId) =>
        {
            await service.MarkAllAsReadAsync(recipientUserId);
            return Results.NoContent();
        });

        return group;
    }
}
