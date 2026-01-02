using ReserveOne.Domain.Entities;

namespace ReserveOne.Application.Tenants;

public interface ITenantRepository
{
    Task AddAsync(Tenant tenant, CancellationToken ct = default);
    Task<Tenant?> GetByApiKeyAsync(string apiKey, CancellationToken ct = default);
}
