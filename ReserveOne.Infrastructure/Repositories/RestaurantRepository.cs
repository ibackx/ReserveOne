using ReserveOne.Application.Restaurants;
using ReserveOne.Domain.Entities;

namespace ReserveOne.Infrastructure.Repositories;

public class RestaurantRepository : IRestaurantRepository
{
    private readonly ReserveOneDbContext _db;

    public RestaurantRepository(ReserveOneDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(Restaurant restaurant, CancellationToken ct = default)
    {
        await _db.Restaurants.AddAsync(restaurant, ct);
        await _db.SaveChangesAsync(ct);
    }
}
