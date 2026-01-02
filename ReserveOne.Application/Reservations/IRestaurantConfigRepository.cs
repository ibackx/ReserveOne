using ReserveOne.Domain.Entities;

namespace ReserveOne.Application.Reservations;

public interface IRestaurantConfigRepository
{
    Task<RestaurantConfig?> GetByRestaurantAsync(Guid restaurantId, TenantContext tenant, CancellationToken ct = default);
}
