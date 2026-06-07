namespace SCM3.Data.Entities.Attributes;

// Strongly-typed shape for System Release CustomAttributes — Associations/Associated
// Items groups only; shared scalar fields (dates, requestor info, etc.) live on Request
// (root CLAUDE.md §5 — Per-Type UI Field Reference, System Release).
public class SystemReleaseAttributes
{
    public List<SegmentAssociation> AssociatedSegments { get; set; } = [];

    public List<SegmentAssociatedItem> AssociatedItemSegments { get; set; } = [];
    public List<CompatibleSoftwareEntry> CompatibleSoftware { get; set; } = [];

    public class SegmentAssociation
    {
        public string? SegmentName { get; set; }
        public string? Version { get; set; }
        public string? State { get; set; }
    }

    public class SegmentAssociatedItem
    {
        public string? SegmentName { get; set; }
        public string? Version { get; set; }
    }

    public class CompatibleSoftwareEntry
    {
        public string? Name { get; set; }
        public string? Version { get; set; }
    }
}
