namespace SCM3.Data.Entities;

// In-app + email notification record. EmailSent confirms the email half of the
// notification succeeded; every notification also gets logged as a history entry
// (root CLAUDE.md §10).
public class Notification
{
    public int NotificationId { get; set; }

    public int RecipientUserId { get; set; }
    public User? Recipient { get; set; }

    public int? RequestId { get; set; }
    public Request? Request { get; set; }

    public string Action { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public bool EmailSent { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime LogDate { get; set; } = DateTime.UtcNow;
}
