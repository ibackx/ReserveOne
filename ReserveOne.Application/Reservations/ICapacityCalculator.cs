using ReserveOne.Domain.Entities;

namespace ReserveOne.Application.Reservations;

public interface ICapacityCalculator
{
    Task<int> GetTotalCapacityAsync(Guid restaurantId, TenantContext tenant, CancellationToken ct = default);
}
