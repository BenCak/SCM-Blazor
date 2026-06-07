using SCM3.Data.Entities;
using SCM3.Data.Repositories;

namespace SCM3.Data.Services;

public class NotificationService(INotificationRepository repository) : INotificationService
{
    public Task<List<Notification>> GetForUserAsync(int recipientUserId, bool unreadOnly = false)
        => repository.GetForUserAsync(recipientUserId, unreadOnly);

    public Task<int> GetUnreadCountAsync(int recipientUserId) => repository.GetUnreadCountAsync(recipientUserId);

    public Task NotifyAsync(int recipientUserId, string action, string message, int? requestId = null)
        => repository.AddAsync(new Notification
        {
            RecipientUserId = recipientUserId,
            RequestId = requestId,
            Action = action,
            Message = message
        });

    public Task MarkAsReadAsync(int notificationId) => repository.MarkAsReadAsync(notificationId);

    public Task MarkAllAsReadAsync(int recipientUserId) => repository.MarkAllAsReadAsync(recipientUserId);
}
