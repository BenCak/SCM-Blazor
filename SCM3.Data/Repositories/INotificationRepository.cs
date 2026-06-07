using SCM3.Data.Entities;

namespace SCM3.Data.Repositories;

public interface INotificationRepository
{
    Task<List<Notification>> GetForUserAsync(int recipientUserId, bool unreadOnly = false);
    Task<int> GetUnreadCountAsync(int recipientUserId);
    Task<Notification?> GetByIdAsync(int id);
    Task AddAsync(Notification entity);
    Task MarkAsReadAsync(int id);
    Task MarkAllAsReadAsync(int recipientUserId);
    Task DeleteAsync(int id); // soft delete — sets IsDeleted = true
}
