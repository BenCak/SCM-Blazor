namespace SCM3.Data.Entities.Attributes;

// Strongly-typed shape for Supplier CSCI Release CustomAttributes — Supplier contact
// scalars plus DCNs/Associated Items groups; shared scalar fields (dates, release
// description, etc.) live on Request. "LRE Part Numbers" is transcribed verbatim from
// the UI spec — distinct from "LRU Part Numbers" used elsewhere, not a typo to "fix"
// (root CLAUDE.md §5 — Per-Type UI Field Reference, Supplier CSCI Release).
public class SupplierCsciReleaseAttributes
{
    public string? SoftwarePointOfContact { get; set; }
    public string? Supplier { get; set; }
    public string? SupplierPointOfContact { get; set; }
    public string? SupplierPocPhone { get; set; }
    public string? SupplierPocEmail { get; set; }
    public string? Location { get; set; }

    public List<DcnEntry> Dcns { get; set; } = [];

    public List<BinaryChecksumEntry> BinaryChecksums { get; set; } = [];
    public List<LrePartNumberEntry> LrePartNumbers { get; set; } = [];

    public class DcnEntry
    {
        public string? DcnNumber { get; set; }
        public string? Title { get; set; }
        public string? Status { get; set; }
    }

    public class BinaryChecksumEntry
    {
        public string? Checksum { get; set; }
        public string? Algorithm { get; set; }
    }

    public class LrePartNumberEntry
    {
        public string? PartNumber { get; set; }
        public string? Description { get; set; }
    }
}
