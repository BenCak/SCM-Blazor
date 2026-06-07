using System.Net.Http.Json;
using SCM3.Data.Entities;

namespace SCM3.Web.Services;

public class NotificationService(HttpClient http) : INotificationService
{
    public async Task<List<Notification>> GetForUserAsync(int recipientUserId, bool unreadOnly = false)
        => await http.GetFromJsonAsync<List<Notification>>($"/api/notifications/user/{recipientUserId}?unreadOnly={unreadOnly}") ?? [];

    public async Task<int> GetUnreadCountAsync(int recipientUserId)
        => await http.GetFromJsonAsync<int>($"/api/notifications/user/{recipientUserId}/unread-count");

    public async Task MarkAsReadAsync(int notificationId)
    {
        var response = await http.PostAsync($"/api/notifications/{notificationId}/read", null);
        response.EnsureSuccessStatusCode();
    }

    public async Task MarkAllAsReadAsync(int recipientUserId)
    {
        var response = await http.PostAsync($"/api/notifications/user/{recipientUserId}/read-all", null);
        response.EnsureSuccessStatusCode();
    }
}
