using Microsoft.EntityFrameworkCore;
using SCM3.Data.Entities;

namespace SCM3.Data.Repositories;

public class RequestRepository(DbContext.SCM3DbContext context) : IRequestRepository
{
    public async Task<List<Request>> GetAllAsync(int? requestTypeId = null, int? requestStatusId = null, int? requestorUserId = null)
    {
        var query = context.Requests
            .Include(r => r.RequestType)
            .Include(r => r.RequestStatus)
            .Include(r => r.System)
            .Include(r => r.Requestor)
            .Where(r => !r.IsDeleted);

        if (requestTypeId.HasValue)
        {
            query = query.Where(r => r.RequestTypeId == requestTypeId.Value);
        }

        if (requestStatusId.HasValue)
        {
            query = query.Where(r => r.RequestStatusId == requestStatusId.Value);
        }

        if (requestorUserId.HasValue)
        {
            query = query.Where(r => r.RequestorUserId == requestorUserId.Value);
        }

        return await query.OrderByDescending(r => r.LogDate).ToListAsync();
    }

    public async Task<List<RequestType>> GetTypesAsync()
        => await context.RequestTypes.Where(t => !t.IsDeleted).OrderBy(t => t.Name).ToListAsync();

    public async Task<List<RequestStatus>> GetStatusesAsync()
        => await context.RequestStatuses.Where(s => !s.IsDeleted).OrderBy(s => s.RequestStatusId).ToListAsync();

    public async Task<Request?> GetByIdAsync(int id)
    {
        return await context.Requests
            .Include(r => r.RequestType)
            .Include(r => r.RequestStatus)
            .Include(r => r.System).ThenInclude(s => s!.Customer)
            .Include(r => r.System).ThenInclude(s => s!.Product)
            .Include(r => r.ParentRequest)
            .Include(r => r.Requestor)
            .FirstOrDefaultAsync(r => r.RequestId == id && !r.IsDeleted);
    }

    public async Task<List<Request>> GetChildrenAsync(int parentRequestId)
    {
        return await context.Requests
            .Include(r => r.RequestType)
            .Include(r => r.RequestStatus)
            .Where(r => r.ParentRequestId == parentRequestId && !r.IsDeleted)
            .OrderBy(r => r.Title)
            .ToListAsync();
    }

    public async Task AddAsync(Request entity)
    {
        context.Requests.Add(entity);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Request entity)
    {
        context.Requests.Update(entity);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await context.Requests.FindAsync(id);
        if (entity is null)
        {
            return;
        }

        entity.IsDeleted = true;
        await context.SaveChangesAsync();
    }

    public async Task<RequestAttributes?> GetAttributesAsync(int requestId)
    {
        return await context.RequestAttributes
            .FirstOrDefaultAsync(a => a.RequestId == requestId && !a.IsDeleted);
    }

    public async Task SaveAttributesAsync(RequestAttributes attributes)
    {
        var existing = await context.RequestAttributes
            .FirstOrDefaultAsync(a => a.RequestId == attributes.RequestId && !a.IsDeleted);

        if (existing is null)
        {
            context.RequestAttributes.Add(attributes);
        }
        else
        {
            existing.CustomAttributes = attributes.CustomAttributes;
        }

        await context.SaveChangesAsync();
    }

    public async Task<List<RequestNote>> GetNotesAsync(int requestId)
    {
        return await context.RequestNotes
            .Include(n => n.Author)
            .Where(n => n.RequestId == requestId && !n.IsDeleted)
            .OrderByDescending(n => n.LogDate)
            .ToListAsync();
    }

    public async Task AddNoteAsync(RequestNote note)
    {
        context.RequestNotes.Add(note);
        await context.SaveChangesAsync();
    }

    public async Task<List<RequestHistory>> GetHistoryAsync(int requestId)
    {
        return await context.RequestHistory
            .Include(h => h.FromStatus)
            .Include(h => h.ToStatus)
            .Include(h => h.ActionBy)
            .Where(h => h.RequestId == requestId && !h.IsDeleted)
            .OrderByDescending(h => h.LogDate)
            .ToListAsync();
    }

    public async Task AddHistoryAsync(RequestHistory entry)
    {
        context.RequestHistory.Add(entry);
        await context.SaveChangesAsync();
    }

    public async Task<List<RequestAttachment>> GetAttachmentsAsync(int requestId)
    {
        return await context.RequestAttachments
            .Include(a => a.UploadedBy)
            .Where(a => a.RequestId == requestId && !a.IsDeleted)
            .OrderByDescending(a => a.LogDate)
            .ToListAsync();
    }

    public async Task AddAttachmentAsync(RequestAttachment attachment)
    {
        context.RequestAttachments.Add(attachment);
        await context.SaveChangesAsync();
    }

    public async Task<RequestSCMStatus?> GetScmStatusAsync(int requestId)
    {
        return await context.RequestSCMStatuses
            .Include(s => s.AssignedTo)
            .FirstOrDefaultAsync(s => s.RequestId == requestId && !s.IsDeleted);
    }

    public async Task SaveScmStatusAsync(RequestSCMStatus status)
    {
        var existing = await context.RequestSCMStatuses
            .FirstOrDefaultAsync(s => s.RequestId == status.RequestId && !s.IsDeleted);

        if (existing is null)
        {
            context.RequestSCMStatuses.Add(status);
        }
        else
        {
            existing.AssignedToUserId = status.AssignedToUserId;
            existing.Status = status.Status;
            existing.ReviewerNotes = status.ReviewerNotes;
            existing.Comments = status.Comments;
            existing.ReleaseReadinessNotes = status.ReleaseReadinessNotes;
        }

        await context.SaveChangesAsync();
    }

    public async Task<List<RequestChangeLog>> GetChangeLogAsync(int requestId)
    {
        return await context.RequestChangeLogs
            .Include(c => c.ChangedBy)
            .Where(c => c.RequestId == requestId && !c.IsDeleted)
            .OrderByDescending(c => c.LogDate)
            .ToListAsync();
    }

    public async Task AddChangeLogEntryAsync(RequestChangeLog entry)
    {
        context.RequestChangeLogs.Add(entry);
        await context.SaveChangesAsync();
    }
}
