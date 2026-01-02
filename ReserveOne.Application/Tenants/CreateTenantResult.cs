namespace ReserveOne.Application.Tenants;

public class CreateTenantResult
{
    public Guid TenantId { get; set; }
    public Guid AdminUserId { get; set; }
    public string AdminTempPassword { get; set; } = string.Empty;
}
