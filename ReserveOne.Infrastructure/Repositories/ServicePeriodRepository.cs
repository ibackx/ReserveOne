using Microsoft.EntityFrameworkCore;
using ReserveOne.Application.Restaurants;
using ReserveOne.Domain.Entities;

namespace ReserveOne.Infrastructure.Repositories;

public class ServicePeriodRepository : IServicePeriodRepository
{
    private readonly ReserveOneDbContext _db;

    public ServicePeriodRepository(ReserveOneDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(ServicePeriod period, CancellationToken ct = default)
    {
        await _db.ServicePeriods.AddAsync(period, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<ServicePeriod>> GetByConfigAsync(Guid restaurantConfigId, Guid tenantId, CancellationToken ct = default)
    {
        return await _db.ServicePeriods
            .Where(sp => sp.RestaurantConfigId == restaurantConfigId && sp.TenantId == tenantId)
            .ToListAsync(ct);
    }
}
