using System.Net.Http.Json;
using SCM3.Data.Entities;

namespace SCM3.Web.Services;

public class RequestService(HttpClient http) : IRequestService
{
    public async Task<List<Request>> GetAllAsync(int? requestTypeId = null, int? requestStatusId = null, int? requestorUserId = null)
    {
        var query = BuildQuery(("requestTypeId", requestTypeId), ("requestStatusId", requestStatusId), ("requestorUserId", requestorUserId));
        return await http.GetFromJsonAsync<List<Request>>($"/api/requests{query}") ?? [];
    }

    public Task<Request?> GetByIdAsync(int id) => http.GetOrDefaultAsync<Request>($"/api/requests/{id}");

    public async Task<List<RequestType>> GetTypesAsync()
        => await http.GetFromJsonAsync<List<RequestType>>("/api/requests/types") ?? [];

    public async Task<List<RequestStatus>> GetStatusesAsync()
        => await http.GetFromJsonAsync<List<RequestStatus>>("/api/requests/statuses") ?? [];

    public async Task<List<Request>> GetChildrenAsync(int parentRequestId)
        => await http.GetFromJsonAsync<List<Request>>($"/api/requests/{parentRequestId}/children") ?? [];

    public async Task SaveAsync(Request request)
    {
        if (request.RequestId == 0)
        {
            var response = await http.PostAsJsonAsync("/api/requests", request);
            response.EnsureSuccessStatusCode();
            var created = await response.Content.ReadFromJsonAsync<Request>();
            if (created is not null)
            {
                request.RequestId = created.RequestId;
            }
        }
        else
        {
            var response = await http.PutAsJsonAsync($"/api/requests/{request.RequestId}", request);
            response.EnsureSuccessStatusCode();
        }
    }

    public async Task DeleteAsync(int id)
    {
        var response = await http.DeleteAsync($"/api/requests/{id}");
        response.EnsureSuccessStatusCode();
    }

    public async Task ChangeStatusAsync(int requestId, int toStatusId, string action, int actionByUserId, string? comment = null)
    {
        var response = await http.PostAsJsonAsync($"/api/requests/{requestId}/status",
            new ChangeStatusBody(toStatusId, action, actionByUserId, comment));
        response.EnsureSuccessStatusCode();
    }

    public Task<RequestAttributes?> GetAttributesAsync(int requestId)
        => http.GetOrDefaultAsync<RequestAttributes>($"/api/requests/{requestId}/attributes");

    public async Task SaveAttributesAsync(RequestAttributes attributes)
    {
        var response = await http.PutAsJsonAsync($"/api/requests/{attributes.RequestId}/attributes", attributes);
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<RequestNote>> GetNotesAsync(int requestId)
        => await http.GetFromJsonAsync<List<RequestNote>>($"/api/requests/{requestId}/notes") ?? [];

    public async Task AddNoteAsync(RequestNote note)
    {
        var response = await http.PostAsJsonAsync($"/api/requests/{note.RequestId}/notes", note);
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<RequestHistory>> GetHistoryAsync(int requestId)
        => await http.GetFromJsonAsync<List<RequestHistory>>($"/api/requests/{requestId}/history") ?? [];

    public async Task<List<RequestAttachment>> GetAttachmentsAsync(int requestId)
        => await http.GetFromJsonAsync<List<RequestAttachment>>($"/api/requests/{requestId}/attachments") ?? [];

    public async Task AddAttachmentAsync(RequestAttachment attachment)
    {
        var response = await http.PostAsJsonAsync($"/api/requests/{attachment.RequestId}/attachments", attachment);
        response.EnsureSuccessStatusCode();
    }

    public Task<RequestSCMStatus?> GetScmStatusAsync(int requestId)
        => http.GetOrDefaultAsync<RequestSCMStatus>($"/api/requests/{requestId}/scm-status");

    public async Task SaveScmStatusAsync(RequestSCMStatus status)
    {
        var response = await http.PutAsJsonAsync($"/api/requests/{status.RequestId}/scm-status", status);
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<RequestChangeLog>> GetChangeLogAsync(int requestId)
        => await http.GetFromJsonAsync<List<RequestChangeLog>>($"/api/requests/{requestId}/change-log") ?? [];

    private static string BuildQuery(params (string Key, int? Value)[] parameters)
    {
        var parts = parameters.Where(p => p.Value.HasValue).Select(p => $"{p.Key}={p.Value}");
        var query = string.Join("&", parts);
        return query.Length == 0 ? string.Empty : $"?{query}";
    }

    // Mirrors SCM3.Api.Endpoints.ChangeStatusRequest's JSON shape — Web doesn't reference
    // SCM3.Api, and System.Text.Json's case-insensitive matching (Web defaults) binds it
    // to the minimal-API record on the other side regardless of naming-policy casing.
    private sealed record ChangeStatusBody(int ToStatusId, string Action, int ActionByUserId, string? Comment);
}
