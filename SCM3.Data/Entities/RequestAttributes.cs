namespace SCM3.Data.Entities;

// One row per request. CustomAttributes holds type-specific fields as a JSON blob whose
// shape varies by RequestType (root CLAUDE.md §5) — e.g. CSCI Supplier carries Supplier
// contact fields while EE Request carries PartNumber/DataRights/etc.
public class RequestAttributes
{
    public int RequestAttributesId { get; set; }

    public int RequestId { get; set; }
    public Request? Request { get; set; }

    public string CustomAttributes { get; set; } = "{}";

    public bool IsDeleted { get; set; }
    public DateTime LogDate { get; set; } = DateTime.UtcNow;
}
