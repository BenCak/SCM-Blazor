namespace SCM3.Data.Entities.Attributes;

// Strongly-typed shape for ASI CSCI Release CustomAttributes — type-unique scalar
// (CsciOnlyRelease) plus DCNs/Associated Items groups; shared scalar fields (dates,
// release description, etc.) live on Request
// (root CLAUDE.md §5 — Per-Type UI Field Reference, ASI CSCI Release).
public class AsiCsciReleaseAttributes
{
    public bool CsciOnlyRelease { get; set; }

    public List<DcnEntry> Dcns { get; set; } = [];

    public List<CommonLibraryEntry> CommonLibraries { get; set; } = [];
    public List<BuildTargetEntry> BuildTargets { get; set; } = [];
    public List<LruPartNumberEntry> LruPartNumbers { get; set; } = [];
    public List<AssociatedThirdPartySoftwareEntry> AssociatedThirdPartySoftware { get; set; } = [];

    public class DcnEntry
    {
        public string? DcnNumber { get; set; }
        public string? Title { get; set; }
        public string? Status { get; set; }
    }

    public class CommonLibraryEntry
    {
        public string? LibraryName { get; set; }
        public string? Version { get; set; }
    }

    public class BuildTargetEntry
    {
        public string? BuildTarget { get; set; }
        public string? Platform { get; set; }
    }

    public class LruPartNumberEntry
    {
        public string? PartNumber { get; set; }
        public string? Description { get; set; }
    }

    public class AssociatedThirdPartySoftwareEntry
    {
        public string? Vendor { get; set; }
        public string? Product { get; set; }
        public string? Version { get; set; }
    }
}
