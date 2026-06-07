namespace SCM3.Data.Entities.Attributes;

// Strongly-typed shape for Third Party Software Request CustomAttributes — Product
// Information + Licensing groups, which take the place of "Release Information" for
// this type (it isn't a "release" in the same sense as the others, and has no
// Relationships section either)
// (root CLAUDE.md §5 — Per-Type UI Field Reference, Third Party Software Request).
public class ThirdPartySoftwareAttributes
{
    // Product Information
    public string? Vendor { get; set; }
    public string? ProductName { get; set; }
    public string? ProductVersion { get; set; }
    public string? CountryOfOrigin { get; set; }
    public string? Source { get; set; }
    public string? ProductUrl { get; set; }
    public string? SourceLocation { get; set; }
    public string? InternalLocation { get; set; }
    public string? DownloadFileNames { get; set; }
    public string? DownloadLocation { get; set; }
    public string? IntendedUse { get; set; }
    public string? Platform { get; set; }
    public string? PointOfContact { get; set; }
    public string? BinarySourceOrBoth { get; set; }

    // Licensing
    public string? LicenseType { get; set; }
    public string? ProductLicense { get; set; }
    public bool? ContainsOss { get; set; }
    public string? Patent { get; set; }
    public string? PatentNonAssertNeeded { get; set; }
    public string? PatentNonAssertApproval { get; set; }
    public string? SoftwareLicenseAgreement { get; set; }
    public string? LicenseUrl { get; set; }
    public string? LicenseTrackingMethod { get; set; }
    public string? OssObligations { get; set; }
    public string? ExportCompliance { get; set; }
    public string? ManagerApproval { get; set; }
    public string? Approver { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public string? Status { get; set; }
}
