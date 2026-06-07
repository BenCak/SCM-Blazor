using SCM3.Data.Entities;

namespace SCM3.Data.Services;

// In-app notifications (root CLAUDE.md §9, §10) — bell-icon feed per user, with a DB
// record per notification. Real-time delivery (SignalR) and the HTML email half of
// "every action triggers" are separate concerns (IEmailService, SignalR hub) layered
// on top of this in SCM3.Web — this service owns the notification record itself.
public interface INotificationService
{
    Task<List<Notification>> GetForUserAsync(int recipientUserId, bool unreadOnly = false);
    Task<int> GetUnreadCountAsync(int recipientUserId);
    Task NotifyAsync(int recipientUserId, string action, string message, int? requestId = null);
    Task MarkAsReadAsync(int notificationId);
    Task MarkAllAsReadAsync(int recipientUserId);
}
