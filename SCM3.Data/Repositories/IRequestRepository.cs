using SCM3.Data.Entities;

namespace SCM3.Data.Repositories;

// Request is the central aggregate (root CLAUDE.md §2) — its detail page surfaces
// related sub-resources (attributes, notes, history, attachments, SCM status, change
// log) directly, so this repository owns those alongside the Request CRUD itself
// rather than splitting each into its own per-entity repository.
public interface IRequestRepository
{
    Task<List<Request>> GetAllAsync(int? requestTypeId = null, int? requestStatusId = null, int? requestorUserId = null);
    Task<Request?> GetByIdAsync(int id);

    // Lookups backing the dashboard's filter chips and type-routed nav pages (root
    // CLAUDE.md §7-8) — small, rarely-changing reference lists owned alongside the
    // Request aggregate they describe rather than split into their own repositories.
    Task<List<RequestType>> GetTypesAsync();
    Task<List<RequestStatus>> GetStatusesAsync();

    Task<List<Request>> GetChildrenAsync(int parentRequestId);
    Task AddAsync(Request entity);
    Task UpdateAsync(Request entity);
    Task DeleteAsync(int id); // soft delete — sets IsDeleted = true

    Task<RequestAttributes?> GetAttributesAsync(int requestId);
    Task SaveAttributesAsync(RequestAttributes attributes);

    Task<List<RequestNote>> GetNotesAsync(int requestId);
    Task AddNoteAsync(RequestNote note);

    Task<List<RequestHistory>> GetHistoryAsync(int requestId);
    Task AddHistoryAsync(RequestHistory entry);

    Task<List<RequestAttachment>> GetAttachmentsAsync(int requestId);
    Task AddAttachmentAsync(RequestAttachment attachment);

    Task<RequestSCMStatus?> GetScmStatusAsync(int requestId);
    Task SaveScmStatusAsync(RequestSCMStatus status);

    Task<List<RequestChangeLog>> GetChangeLogAsync(int requestId);
    Task AddChangeLogEntryAsync(RequestChangeLog entry);
}
