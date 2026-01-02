namespace ReserveOne.Application;

public class TenantContext
{
    public Guid? TenantId { get; }
    public Guid UserId { get; }

    public TenantContext(Guid? tenantId, Guid userId)
    {
        TenantId = tenantId;
        UserId = userId;
    }
}
