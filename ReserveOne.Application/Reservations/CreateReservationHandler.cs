using ReserveOne.Domain.Entities;
using ReserveOne.Domain.Enums;

namespace ReserveOne.Application.Reservations;

public class CreateReservationHandler
{
    private readonly IReservationRepository _repository;
    private readonly TenantContext _tenantContext;
    private readonly AvailabilityService _availability;

    public CreateReservationHandler(IReservationRepository repository, TenantContext tenantContext, AvailabilityService availability)
    {
        _repository = repository;
        _tenantContext = tenantContext;
        _availability = availability;
    }

    public async Task<Guid> HandleAsync(CreateReservationCommand command, CancellationToken cancellationToken = default)
    {
        if (_tenantContext.TenantId is null)
            throw new InvalidOperationException("Operacion requiere TenantId");

        // Validaciones simples
        if (string.IsNullOrWhiteSpace(command.CustomerName))
            throw new ArgumentException("CustomerName requerido");
        if (command.PartySize <= 0)
            throw new ArgumentException("PartySize inválido");
        if (command.Fecha == default)
            throw new ArgumentException("Fecha requerida");

        // Reglas de disponibilidad
        await _availability.CheckAvailabilityAsync(command, _tenantContext, cancellationToken);

        // Asignación de mesa automática (stub)
        Guid? tableId = null; // TODO: lógica de asignación automática

        var reservation = new Reservation
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantContext.TenantId,
            Nombre = $"Reserva {command.CustomerName}",
            Borrado = false,
            FechaCreacion = DateTime.UtcNow,
            UltimaFechaEdicion = DateTime.UtcNow,
            IdCreador = _tenantContext.UserId,
            IdUltimoEditor = _tenantContext.UserId,
            RestaurantId = command.RestaurantId,
            SalonId = command.SalonId,
            TableId = tableId,
            CustomerName = command.CustomerName,
            Phone = command.Phone,
            Fecha = command.Fecha,
            Hora = command.Hora,
            PartySize = command.PartySize,
            Status = ReservationStatus.Pending,
            Source = command.Source,
        };

        await _repository.AddAsync(reservation, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return reservation.Id;
    }
}
