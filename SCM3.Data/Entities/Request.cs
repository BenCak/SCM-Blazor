namespace SCM3.Data.Entities;

// Core shared fields for every request type. The Request row IS the SystemVersion for
// System-type requests, and the hierarchy (Segment -> System, CSCI -> Segment) is modeled
// via the self-referencing ParentRequestId (root CLAUDE.md §2).
public class Request
{
    public int RequestId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Version { get; set; }

    public int RequestTypeId { get; set; }
    public RequestType? RequestType { get; set; }

    public int RequestStatusId { get; set; }
    public RequestStatus? RequestStatus { get; set; }

    // Required for System/Segment/CSCI requests, NULL for EE/TE/Third Party (root CLAUDE.md §2)
    public int? SystemId { get; set; }
    public SystemEntity? System { get; set; }

    public int? ParentRequestId { get; set; }
    public Request? ParentRequest { get; set; }

    public int RequestorUserId { get; set; }
    public User? Requestor { get; set; }

    // Snapshot of the requestor's contact details at submission time — kept on the
    // request itself (rather than read live off User) so the record stays accurate
    // even if the requestor's account details change later (root CLAUDE.md §5 Release
    // Information / Requestor Information field reference).
    public string? RequestorName { get; set; }
    public string? RequestorEmail { get; set; }
    public string? RequestorPhone { get; set; }

    public string? ParentVersion { get; set; }

    public string? ChargeNumber { get; set; }
    public string? Priority { get; set; }
    public DateTime? SubmittedDate { get; set; }
    public DateTime? ReadyDate { get; set; }
    public DateTime? NeedDate { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public string? ReleaseDescription { get; set; }
    public string? NotesToScm { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime LogDate { get; set; } = DateTime.UtcNow;
}
