using ReserveOne.Domain.Entities;

namespace ReserveOne.Application.Restaurants;

public class CreateSalonHandler
{
    private readonly ISalonRepository _salons;
    private readonly TenantContext _tenant;

    public CreateSalonHandler(ISalonRepository salons, TenantContext tenant)
    {
        _salons = salons;
        _tenant = tenant;
    }

    public async Task<Guid> HandleAsync(CreateSalonCommand cmd, CancellationToken ct = default)
    {
        if (_tenant.TenantId is null) throw new InvalidOperationException("Operacion requiere TenantId");
        if (cmd.RestaurantId == Guid.Empty) throw new ArgumentException("RestaurantId requerido");
        if (string.IsNullOrWhiteSpace(cmd.Nombre)) throw new ArgumentException("Nombre requerido");

        var salon = new Salon
        {
            Id = Guid.NewGuid(),
            TenantId = _tenant.TenantId,
            RestaurantId = cmd.RestaurantId,
            Nombre = cmd.Nombre,
            Prioridad = cmd.Prioridad,
            Borrado = false,
            FechaCreacion = DateTime.UtcNow,
            UltimaFechaEdicion = DateTime.UtcNow,
            IdCreador = _tenant.UserId,
            IdUltimoEditor = _tenant.UserId,
        };

        await _salons.AddAsync(salon, ct);
        return salon.Id;
    }
}
