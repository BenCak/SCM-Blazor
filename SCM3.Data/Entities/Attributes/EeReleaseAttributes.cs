namespace SCM3.Data.Entities.Attributes;

// Strongly-typed shape for Electrical Engineering Release CustomAttributes — type-unique
// scalars only; shared scalar fields (Charge Number, dates, release description, etc.)
// live on Request (root CLAUDE.md §5 — Per-Type UI Field Reference, EE Release).
public class EeReleaseAttributes
{
    public string? PartNumber { get; set; }
    public string? AssemblyPartNumber { get; set; }
    public string? AssemblyDescription { get; set; }
    public string? DataRights { get; set; }
    public string? ChangeNotice { get; set; }
    public string? ReleaseLocation { get; set; }
}
