namespace SCM3.Data.Entities;

public class RequestAttachment
{
    public int RequestAttachmentId { get; set; }

    public int RequestId { get; set; }
    public Request? Request { get; set; }

    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string? ContentType { get; set; }

    public int UploadedByUserId { get; set; }
    public User? UploadedBy { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime LogDate { get; set; } = DateTime.UtcNow;
}
