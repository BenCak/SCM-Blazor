namespace SCM3.Data.Entities;

// State-transition log per request — one row per workflow move (root CLAUDE.md §6),
// e.g. Pending -> In Review via the "Pick Up" action.
public class RequestHistory
{
    public int RequestHistoryId { get; set; }

    public int RequestId { get; set; }
    public Request? Request { get; set; }

    public int? FromStatusId { get; set; }
    public RequestStatus? FromStatus { get; set; }

    public int ToStatusId { get; set; }
    public RequestStatus? ToStatus { get; set; }

    public string Action { get; set; } = string.Empty;

    public int ActionByUserId { get; set; }
    public User? ActionBy { get; set; }

    public string? Comment { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime LogDate { get; set; } = DateTime.UtcNow;
}
