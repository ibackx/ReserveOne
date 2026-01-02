namespace ReserveOne.Domain.Entities;

public class LogEntry
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string Operation { get; set; } = string.Empty;
    public string Changes { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Guid UserId { get; set; }
}
