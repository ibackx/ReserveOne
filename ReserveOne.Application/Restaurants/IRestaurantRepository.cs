using ReserveOne.Domain.Entities;

namespace ReserveOne.Application.Restaurants;

public interface IRestaurantRepository
{
    Task AddAsync(Restaurant restaurant, CancellationToken ct = default);
}
