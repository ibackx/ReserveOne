using ReserveOne.Domain.Entities;

namespace ReserveOne.Application.Reservations;

public interface IReservationRepository
{
    Task AddAsync(Reservation reservation, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<Reservation?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Reservation>> GetByFilterAsync(Guid tenantId, Guid? restaurantId, DateTime? date, CancellationToken cancellationToken = default);
}
