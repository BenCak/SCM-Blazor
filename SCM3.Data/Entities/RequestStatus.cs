namespace SCM3.Data.Entities;

public class RequestStatus
{
    public int RequestStatusId { get; set; }
    public string Name { get; set; } = string.Empty;

    public bool IsDeleted { get; set; }
    public DateTime LogDate { get; set; } = DateTime.UtcNow;
}
