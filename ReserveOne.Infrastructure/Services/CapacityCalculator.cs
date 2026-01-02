using Microsoft.EntityFrameworkCore;
using ReserveOne.Application;
using ReserveOne.Application.Reservations;

namespace ReserveOne.Infrastructure.Services;

public class CapacityCalculator : ICapacityCalculator
{
    private readonly ReserveOneDbContext _db;

    public CapacityCalculator(ReserveOneDbContext db)
    {
        _db = db;
    }

    public async Task<int> GetTotalCapacityAsync(Guid restaurantId, TenantContext tenant, CancellationToken ct = default)
    {
        var config = await _db.RestaurantConfigs
            .Where(rc => rc.RestaurantId == restaurantId && rc.TenantId == tenant.TenantId && rc.IsActive)
            .FirstOrDefaultAsync(ct);

        if (config == null) return 0;

        if (config.AutoCalculateCapacity)
        {
            var seats = await _db.Tables
                .Where(t => t.TenantId == tenant.TenantId)
                .Join(_db.Salons.Where(s => s.RestaurantId == restaurantId && s.TenantId == tenant.TenantId), t => t.SalonId, s => s.Id, (t, s) => t)
                .SumAsync(t => (int?)t.Capacidad, ct) ?? 0;
            return seats;
        }

        return config.MaxSeatsOverride ?? 0;
    }
}
