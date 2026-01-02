using ReserveOne.Application.Restaurants;
using ReserveOne.Domain.Entities;

namespace ReserveOne.Infrastructure.Repositories;

public class SalonRepository : ISalonRepository
{
    private readonly ReserveOneDbContext _db;

    public SalonRepository(ReserveOneDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(Salon salon, CancellationToken ct = default)
    {
        await _db.Salons.AddAsync(salon, ct);
        await _db.SaveChangesAsync(ct);
    }
}
