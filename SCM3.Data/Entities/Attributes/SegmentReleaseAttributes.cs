namespace SCM3.Data.Entities.Attributes;

// Strongly-typed shape for Segment Release CustomAttributes — Associations group only;
// shared scalar fields (dates, parent system, etc.) live on Request
// (root CLAUDE.md §5 — Per-Type UI Field Reference, Segment Release).
public class SegmentReleaseAttributes
{
    public List<CsciAssociation> AssociatedCscis { get; set; } = [];

    public class CsciAssociation
    {
        public string? CsciName { get; set; }
        public string? Version { get; set; }
        public string? State { get; set; }
    }
}
