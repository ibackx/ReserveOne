using ReserveOne.Domain.Entities;

namespace ReserveOne.Application.Reservations;

public interface IReadOnlyReservationRepository
{
    Task<IReadOnlyList<Reservation>> GetOverlappingAsync(Guid restaurantId, DateTime date, TimeSpan start, TimeSpan end, TenantContext tenant, CancellationToken ct = default);
}
