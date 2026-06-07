using SCM3.Data.Entities;

namespace SCM3.Data.Services;

// NOTE: Root CLAUDE.md §9 lists service interfaces under SCM3.Web/Services with
// implementations here in SCM3.Data/Services. That split isn't buildable as written —
// SCM3.Web already references SCM3.Data, so SCM3.Data can't also reference SCM3.Web's
// interfaces without a circular project reference. Interfaces live next to their
// implementations here instead; SCM3.Web consumes them through its existing reference
// to SCM3.Data when registering them in Program.cs.

// CRUD + filtering for requests, plus the sub-resources shown on the request detail
// page — attributes, notes, history, attachments, SCM status, change log (root CLAUDE.md §9).
public interface IRequestService
{
    Task<List<Request>> GetAllAsync(int? requestTypeId = null, int? requestStatusId = null, int? requestorUserId = null);
    Task<Request?> GetByIdAsync(int id);

    // Lookups backing the dashboard's filter chips and type-routed nav pages (root CLAUDE.md §7-8)
    Task<List<RequestType>> GetTypesAsync();
    Task<List<RequestStatus>> GetStatusesAsync();

    Task<List<Request>> GetChildrenAsync(int parentRequestId);
    Task SaveAsync(Request request);
    Task DeleteAsync(int id);

    Task<RequestAttributes?> GetAttributesAsync(int requestId);
    Task SaveAttributesAsync(RequestAttributes attributes);

    Task<List<RequestNote>> GetNotesAsync(int requestId);
    Task AddNoteAsync(RequestNote note);

    Task<List<RequestHistory>> GetHistoryAsync(int requestId);

    Task<List<RequestAttachment>> GetAttachmentsAsync(int requestId);
    Task AddAttachmentAsync(RequestAttachment attachment);

    Task<RequestSCMStatus?> GetScmStatusAsync(int requestId);
    Task SaveScmStatusAsync(RequestSCMStatus status);

    Task<List<RequestChangeLog>> GetChangeLogAsync(int requestId);

    // Records a workflow transition (root CLAUDE.md §6) — updates the request's status
    // and appends the corresponding RequestHistory entry in one step.
    Task ChangeStatusAsync(int requestId, int toStatusId, string action, int actionByUserId, string? comment = null);
}
