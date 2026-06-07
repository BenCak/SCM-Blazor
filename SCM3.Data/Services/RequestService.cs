using SCM3.Data.Entities;
using SCM3.Data.Repositories;

namespace SCM3.Data.Services;

public class RequestService(IRequestRepository repository) : IRequestService
{
    public Task<List<Request>> GetAllAsync(int? requestTypeId = null, int? requestStatusId = null, int? requestorUserId = null)
        => repository.GetAllAsync(requestTypeId, requestStatusId, requestorUserId);

    public Task<Request?> GetByIdAsync(int id) => repository.GetByIdAsync(id);

    public Task<List<RequestType>> GetTypesAsync() => repository.GetTypesAsync();

    public Task<List<RequestStatus>> GetStatusesAsync() => repository.GetStatusesAsync();

    public Task<List<Request>> GetChildrenAsync(int parentRequestId) => repository.GetChildrenAsync(parentRequestId);

    public Task SaveAsync(Request request)
        => request.RequestId == 0 ? repository.AddAsync(request) : repository.UpdateAsync(request);

    public Task DeleteAsync(int id) => repository.DeleteAsync(id);

    public Task<RequestAttributes?> GetAttributesAsync(int requestId) => repository.GetAttributesAsync(requestId);

    public Task SaveAttributesAsync(RequestAttributes attributes) => repository.SaveAttributesAsync(attributes);

    public Task<List<RequestNote>> GetNotesAsync(int requestId) => repository.GetNotesAsync(requestId);

    public Task AddNoteAsync(RequestNote note) => repository.AddNoteAsync(note);

    public Task<List<RequestHistory>> GetHistoryAsync(int requestId) => repository.GetHistoryAsync(requestId);

    public Task<List<RequestAttachment>> GetAttachmentsAsync(int requestId) => repository.GetAttachmentsAsync(requestId);

    public Task AddAttachmentAsync(RequestAttachment attachment) => repository.AddAttachmentAsync(attachment);

    public Task<RequestSCMStatus?> GetScmStatusAsync(int requestId) => repository.GetScmStatusAsync(requestId);

    public Task SaveScmStatusAsync(RequestSCMStatus status) => repository.SaveScmStatusAsync(status);

    public Task<List<RequestChangeLog>> GetChangeLogAsync(int requestId) => repository.GetChangeLogAsync(requestId);

    public async Task ChangeStatusAsync(int requestId, int toStatusId, string action, int actionByUserId, string? comment = null)
    {
        var request = await repository.GetByIdAsync(requestId);
        if (request is null)
        {
            return;
        }

        var fromStatusId = request.RequestStatusId;
        request.RequestStatusId = toStatusId;
        await repository.UpdateAsync(request);

        await repository.AddHistoryAsync(new RequestHistory
        {
            RequestId = requestId,
            FromStatusId = fromStatusId,
            ToStatusId = toStatusId,
            Action = action,
            ActionByUserId = actionByUserId,
            Comment = comment
        });
    }
}
