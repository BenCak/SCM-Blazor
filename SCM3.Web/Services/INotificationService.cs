using SCM3.Data.Entities;

namespace SCM3.Web.Services;

// In-app notifications (root CLAUDE.md §9, §10) — bell-icon feed per user. HTTP-client-
// backed wrapper around SCM3.Api's /api/notifications endpoint group. Notification
// records are created server-side (as a consequence of request actions going through
// SCM3.Api), so unlike SCM3.Data.Services.INotificationService there's no NotifyAsync
// here — Web only ever reads the feed and updates read-state.
public interface INotificationService
{
    Task<List<Notification>> GetForUserAsync(int recipientUserId, bool unreadOnly = false);
    Task<int> GetUnreadCountAsync(int recipientUserId);
    Task MarkAsReadAsync(int notificationId);
    Task MarkAllAsReadAsync(int recipientUserId);
}
