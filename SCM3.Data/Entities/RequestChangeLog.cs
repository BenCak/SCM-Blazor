namespace SCM3.Data.Entities;

// Field-level change audit per request — feeds the Change Log slide-in panel,
// visible to SCM_Staff/SCM_Admin only (root CLAUDE.md §7).
public class RequestChangeLog
{
    public int RequestChangeLogId { get; set; }

    public int RequestId { get; set; }
    public Request? Request { get; set; }

    public string FieldName { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }

    public int ChangedByUserId { get; set; }
    public User? ChangedBy { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime LogDate { get; set; } = DateTime.UtcNow;
}
