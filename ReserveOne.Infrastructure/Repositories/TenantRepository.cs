using Microsoft.EntityFrameworkCore;
using ReserveOne.Application.Tenants;
using ReserveOne.Domain.Entities;

namespace ReserveOne.Infrastructure.Repositories;

public class TenantRepository : ITenantRepository
{
    private readonly ReserveOneDbContext _db;

    public TenantRepository(ReserveOneDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(Tenant tenant, CancellationToken ct = default)
    {
        await _db.Tenants.AddAsync(tenant, ct);
        await _db.SaveChangesAsync(ct);
    }

    public Task<Tenant?> GetByApiKeyAsync(string apiKey, CancellationToken ct = default)
    {
        return _db.Tenants.FirstOrDefaultAsync(t => t.ApiKey == apiKey, ct);
    }
}
