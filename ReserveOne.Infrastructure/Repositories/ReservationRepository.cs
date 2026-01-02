using Microsoft.EntityFrameworkCore;
using ReserveOne.Application.Reservations;
using ReserveOne.Domain.Entities;

namespace ReserveOne.Infrastructure.Repositories;

public class ReservationRepository : IReservationRepository
{
    private readonly ReserveOneDbContext _db;

    public ReservationRepository(ReserveOneDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(Reservation reservation, CancellationToken cancellationToken = default)
    {
        await _db.Reservations.AddAsync(reservation, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<Reservation?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _db.Reservations.FirstOrDefaultAsync(r => r.Id == id && r.TenantId == tenantId, cancellationToken);
    }

    public async Task<IReadOnlyList<Reservation>> GetByFilterAsync(Guid tenantId, Guid? restaurantId, DateTime? date, CancellationToken cancellationToken = default)
    {
        var query = _db.Reservations.AsQueryable().Where(r => r.TenantId == tenantId);
        if (restaurantId.HasValue) query = query.Where(r => r.RestaurantId == restaurantId.Value);
        if (date.HasValue)
        {
            var day = date.Value.Date;
            var nextDay = day.AddDays(1);
            query = query.Where(r => r.Fecha >= day && r.Fecha < nextDay);
        }
        return await query.OrderBy(r => r.Fecha).ThenBy(r => r.Hora).ToListAsync(cancellationToken);
    }
}
