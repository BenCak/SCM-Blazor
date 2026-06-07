namespace SCM3.Data.Entities;

// Backs the SVM Notes tab — newest on top, always accessible to SCM (root CLAUDE.md §7).
public class RequestNote
{
    public int RequestNoteId { get; set; }

    public int RequestId { get; set; }
    public Request? Request { get; set; }

    public int AuthorUserId { get; set; }
    public User? Author { get; set; }

    public string NoteText { get; set; } = string.Empty;

    public bool IsDeleted { get; set; }
    public DateTime LogDate { get; set; } = DateTime.UtcNow;
}
