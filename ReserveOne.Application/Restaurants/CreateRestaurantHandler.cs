using ReserveOne.Domain.Entities;

namespace ReserveOne.Application.Restaurants;

public class CreateRestaurantHandler
{
    private readonly IRestaurantRepository _repo;
    private readonly TenantContext _tenant;

    public CreateRestaurantHandler(IRestaurantRepository repo, TenantContext tenant)
    {
        _repo = repo;
        _tenant = tenant;
    }

    public async Task<Guid> HandleAsync(CreateRestaurantCommand cmd, CancellationToken ct = default)
    {
        if (_tenant.TenantId is null) throw new InvalidOperationException("Operacion requiere TenantId");
        if (string.IsNullOrWhiteSpace(cmd.Nombre)) throw new ArgumentException("Nombre requerido");

        var restaurant = new Restaurant
        {
            Id = Guid.NewGuid(),
            TenantId = _tenant.TenantId,
            Nombre = cmd.Nombre,
            TimeZone = cmd.TimeZone,
            Borrado = false,
            FechaCreacion = DateTime.UtcNow,
            UltimaFechaEdicion = DateTime.UtcNow,
            IdCreador = _tenant.UserId,
            IdUltimoEditor = _tenant.UserId,
        };
        await _repo.AddAsync(restaurant, ct);
        return restaurant.Id;
    }
}
