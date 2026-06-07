using Microsoft.EntityFrameworkCore;
using SCM3.Data.Entities;

namespace SCM3.Data.Repositories;

public class NotificationRepository(DbContext.SCM3DbContext context) : INotificationRepository
{
    public async Task<List<Notification>> GetForUserAsync(int recipientUserId, bool unreadOnly = false)
    {
        var query = context.Notifications
            .Include(n => n.Request)
            .Where(n => n.RecipientUserId == recipientUserId && !n.IsDeleted);

        if (unreadOnly)
        {
            query = query.Where(n => !n.IsRead);
        }

        return await query.OrderByDescending(n => n.LogDate).ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync(int recipientUserId)
    {
        return await context.Notifications
            .CountAsync(n => n.RecipientUserId == recipientUserId && !n.IsRead && !n.IsDeleted);
    }

    public async Task<Notification?> GetByIdAsync(int id)
    {
        return await context.Notifications
            .Include(n => n.Request)
            .FirstOrDefaultAsync(n => n.NotificationId == id && !n.IsDeleted);
    }

    public async Task AddAsync(Notification entity)
    {
        context.Notifications.Add(entity);
        await context.SaveChangesAsync();
    }

    public async Task MarkAsReadAsync(int id)
    {
        var entity = await context.Notifications.FindAsync(id);
        if (entity is null)
        {
            return;
        }

        entity.IsRead = true;
        await context.SaveChangesAsync();
    }

    public async Task MarkAllAsReadAsync(int recipientUserId)
    {
        await context.Notifications
            .Where(n => n.RecipientUserId == recipientUserId && !n.IsRead && !n.IsDeleted)
            .ExecuteUpdateAsync(setters => setters.SetProperty(n => n.IsRead, true));
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await context.Notifications.FindAsync(id);
        if (entity is null)
        {
            return;
        }

        entity.IsDeleted = true;
        await context.SaveChangesAsync();
    }
}
