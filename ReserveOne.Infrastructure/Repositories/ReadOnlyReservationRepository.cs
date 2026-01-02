using Microsoft.EntityFrameworkCore;
using ReserveOne.Application;
using ReserveOne.Application.Reservations;
using ReserveOne.Domain.Entities;

namespace ReserveOne.Infrastructure.Repositories;

public class ReadOnlyReservationRepository : IReadOnlyReservationRepository
{
    private readonly ReserveOneDbContext _db;

    public ReadOnlyReservationRepository(ReserveOneDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<Reservation>> GetOverlappingAsync(Guid restaurantId, DateTime date, TimeSpan start, TimeSpan end, TenantContext tenant, CancellationToken ct = default)
    {
        var day = date.Date;
        var nextDay = day.AddDays(1);

        return await _db.Reservations
            .Where(r => r.RestaurantId == restaurantId && r.TenantId == tenant.TenantId && r.Fecha >= day && r.Fecha < nextDay)
            .ToListAsync(ct);
    }
}
