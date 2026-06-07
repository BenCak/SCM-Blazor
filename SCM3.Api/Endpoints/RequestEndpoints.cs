using SCM3.Data.Entities;
using SCM3.Data.Services;

namespace SCM3.Api.Endpoints;

// CRUD + filtering for requests, plus the sub-resources shown on the request detail page
// (root CLAUDE.md §9, §7) — attributes, notes, history, attachments, SCM status, change
// log, and workflow status transitions (root CLAUDE.md §6).
public static class RequestEndpoints
{
    public static RouteGroupBuilder MapRequestEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/requests").WithTags("Requests");

        group.MapGet("/", async (IRequestService service, int? requestTypeId, int? requestStatusId, int? requestorUserId)
            => Results.Ok(await service.GetAllAsync(requestTypeId, requestStatusId, requestorUserId)));

        group.MapGet("/types", async (IRequestService service)
            => Results.Ok(await service.GetTypesAsync()));

        group.MapGet("/statuses", async (IRequestService service)
            => Results.Ok(await service.GetStatusesAsync()));

        group.MapGet("/{id:int}", async (IRequestService service, int id)
            => await service.GetByIdAsync(id) is { } request ? Results.Ok(request) : Results.NotFound());

        group.MapGet("/{id:int}/children", async (IRequestService service, int id)
            => Results.Ok(await service.GetChildrenAsync(id)));

        group.MapPost("/", async (IRequestService service, Request request) =>
        {
            await service.SaveAsync(request);
            return Results.Created($"/api/requests/{request.RequestId}", request);
        });

        group.MapPut("/{id:int}", async (IRequestService service, int id, Request request) =>
        {
            request.RequestId = id;
            await service.SaveAsync(request);
            return Results.NoContent();
        });

        group.MapDelete("/{id:int}", async (IRequestService service, int id) =>
        {
            await service.DeleteAsync(id);
            return Results.NoContent();
        });

        group.MapPost("/{id:int}/status", async (IRequestService service, int id, ChangeStatusRequest body) =>
        {
            await service.ChangeStatusAsync(id, body.ToStatusId, body.Action, body.ActionByUserId, body.Comment);
            return Results.NoContent();
        });

        // --- Attributes (1:1 — CustomAttributes JSON shape varies by request type) ---
        group.MapGet("/{id:int}/attributes", async (IRequestService service, int id)
            => await service.GetAttributesAsync(id) is { } attributes ? Results.Ok(attributes) : Results.NotFound());

        group.MapPut("/{id:int}/attributes", async (IRequestService service, int id, RequestAttributes attributes) =>
        {
            attributes.RequestId = id;
            await service.SaveAttributesAsync(attributes);
            return Results.NoContent();
        });

        // --- SVM Notes ---
        group.MapGet("/{id:int}/notes", async (IRequestService service, int id)
            => Results.Ok(await service.GetNotesAsync(id)));

        group.MapPost("/{id:int}/notes", async (IRequestService service, int id, RequestNote note) =>
        {
            note.RequestId = id;
            await service.AddNoteAsync(note);
            return Results.Created($"/api/requests/{id}/notes", note);
        });

        // --- History (read-only — written via the status-transition endpoint above) ---
        group.MapGet("/{id:int}/history", async (IRequestService service, int id)
            => Results.Ok(await service.GetHistoryAsync(id)));

        // --- Attachments ---
        group.MapGet("/{id:int}/attachments", async (IRequestService service, int id)
            => Results.Ok(await service.GetAttachmentsAsync(id)));

        group.MapPost("/{id:int}/attachments", async (IRequestService service, int id, RequestAttachment attachment) =>
        {
            attachment.RequestId = id;
            await service.AddAttachmentAsync(attachment);
            return Results.Created($"/api/requests/{id}/attachments", attachment);
        });

        // --- SCM Status (internal SCM working status, distinct from workflow status) ---
        group.MapGet("/{id:int}/scm-status", async (IRequestService service, int id)
            => await service.GetScmStatusAsync(id) is { } status ? Results.Ok(status) : Results.NotFound());

        group.MapPut("/{id:int}/scm-status", async (IRequestService service, int id, RequestSCMStatus status) =>
        {
            status.RequestId = id;
            await service.SaveScmStatusAsync(status);
            return Results.NoContent();
        });

        // --- Change Log (read-only — field-level audit, written by the service layer) ---
        group.MapGet("/{id:int}/change-log", async (IRequestService service, int id)
            => Results.Ok(await service.GetChangeLogAsync(id)));

        return group;
    }
}

// Request body for the workflow-transition endpoint (root CLAUDE.md §6 Valid Transitions).
public record ChangeStatusRequest(int ToStatusId, string Action, int ActionByUserId, string? Comment);
