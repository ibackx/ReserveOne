using Microsoft.EntityFrameworkCore;
using ReserveOne.Application;
using ReserveOne.Application.Reservations;
using ReserveOne.Domain.Entities;

namespace ReserveOne.Infrastructure.Repositories;

public class RestaurantConfigRepository : IRestaurantConfigRepository
{
    private readonly ReserveOneDbContext _db;

    public RestaurantConfigRepository(ReserveOneDbContext db)
    {
        _db = db;
    }

    public async Task<RestaurantConfig?> GetByRestaurantAsync(Guid restaurantId, TenantContext tenant, CancellationToken ct = default)
    {
        return await _db.RestaurantConfigs
            .Include(rc => rc.ServicePeriods)
            .Where(rc => rc.RestaurantId == restaurantId && rc.TenantId == tenant.TenantId)
            .FirstOrDefaultAsync(ct);
    }
}
