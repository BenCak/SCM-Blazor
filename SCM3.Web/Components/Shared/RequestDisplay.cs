namespace SCM3.Web.Components.Shared;

// Shared lookup for rendering requests consistently across the master list and detail
// panel — keeps the status-color mapping (root CLAUDE.md §7-8) in one place.
internal static class RequestDisplay
{
    public static string StatusCssClass(string? statusName) => statusName switch
    {
        "Draft" => "scm3-status-draft",
        "Pending" => "scm3-status-pending",
        "In Review" => "scm3-status-in-review",
        "Released" => "scm3-status-released",
        "Rejected" => "scm3-status-rejected",
        "Terminated" => "scm3-status-terminated",
        _ => "scm3-status-default"
    };

    // Telerik ThemeColor string for TelerikChip status badge — maps each workflow
    // status to a Telerik semantic color so chips carry meaning without custom CSS.
    public static string StatusThemeColor(string? statusName) => statusName switch
    {
        "Pending"   => "warning",
        "In Review" => "info",
        "Released"  => "success",
        "Rejected"  => "error",
        "Terminated" => "error",
        _           => "base"
    };
}
