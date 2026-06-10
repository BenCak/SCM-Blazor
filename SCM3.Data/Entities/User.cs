namespace SCM3.Data.Entities;

public class User
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Department { get; set; }

    // Single role per user — not many-to-many (root CLAUDE.md §2)
    public string Role { get; set; } = string.Empty;

    // Preferred Telerik theme key — see SCM3.Web.Services.ThemeCatalog (e.g. "default", "default-ocean-blue")
    public string Theme { get; set; } = "default";

    public bool IsDeleted { get; set; }
    public DateTime LogDate { get; set; } = DateTime.UtcNow;
}
