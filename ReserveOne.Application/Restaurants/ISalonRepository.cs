using ReserveOne.Domain.Entities;

namespace ReserveOne.Application.Restaurants;

public interface ISalonRepository
{
    Task AddAsync(Salon salon, CancellationToken ct = default);
}
