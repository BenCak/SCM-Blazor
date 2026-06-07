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

    // Preferred Telerik theme, e.g. "scm3-gasi", "scm3-gasi-dark"
    public string Theme { get; set; } = "scm3-gasi";

    public bool IsDeleted { get; set; }
    public DateTime LogDate { get; set; } = DateTime.UtcNow;
}
