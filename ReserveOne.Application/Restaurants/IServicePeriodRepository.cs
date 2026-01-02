using ReserveOne.Domain.Entities;

namespace ReserveOne.Application.Restaurants;

public interface IServicePeriodRepository
{
    Task AddAsync(ServicePeriod period, CancellationToken ct = default);
    Task<IReadOnlyList<ServicePeriod>> GetByConfigAsync(Guid restaurantConfigId, Guid tenantId, CancellationToken ct = default);
}
