namespace ReserveOne.Application.Tenants;

public class CreateTenantCommand
{
    public string TenantName { get; set; } = string.Empty;
    public string AdminEmail { get; set; } = string.Empty;
    public string AdminName { get; set; } = string.Empty;
    public string Plan { get; set; } = string.Empty;
}
