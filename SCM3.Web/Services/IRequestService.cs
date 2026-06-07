using SCM3.Data.Entities;

namespace SCM3.Web.Services;

// CRUD + filtering for requests, plus the sub-resources shown on the request detail
// page — attributes, notes, history, attachments, SCM status, change log (root
// CLAUDE.md §9). HTTP-client-backed wrapper around SCM3.Api's /api/requests endpoint
// group (Web -> Api -> Data -> DB — root CLAUDE.md architecture pivot).
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
