namespace SCM3.Data.Entities;

// SCM team's internal working status/assignment for a request — distinct from the
// requestor-facing workflow status on Request.RequestStatusId (root CLAUDE.md §2).
public class RequestSCMStatus
{
    public int RequestSCMStatusId { get; set; }

    public int RequestId { get; set; }
    public Request? Request { get; set; }

    public int? AssignedToUserId { get; set; }
    public User? AssignedTo { get; set; }

    public string? Status { get; set; }

    // SCM Status field reference (root CLAUDE.md §5 — Per-Type UI Field Reference):
    // ReviewerNotes/Comments are common to every type's SCM Status section;
    // ReleaseReadinessNotes is unique to System Release.
    public string? ReviewerNotes { get; set; }
    public string? Comments { get; set; }
    public string? ReleaseReadinessNotes { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime LogDate { get; set; } = DateTime.UtcNow;
}
