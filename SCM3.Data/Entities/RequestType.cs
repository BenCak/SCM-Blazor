namespace SCM3.Data.Entities;

public class RequestType
{
    public int RequestTypeId { get; set; }
    public string Name { get; set; } = string.Empty;

    public bool IsDeleted { get; set; }
    public DateTime LogDate { get; set; } = DateTime.UtcNow;
}
