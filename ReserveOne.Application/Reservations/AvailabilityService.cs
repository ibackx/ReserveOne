using ReserveOne.Domain.Entities;

namespace ReserveOne.Application.Reservations;

public class AvailabilityService
{
    private readonly IRestaurantConfigRepository _configRepo;
    private readonly IReadOnlyReservationRepository _readRepo;
    private readonly ICapacityCalculator _capacity;

    public AvailabilityService(IRestaurantConfigRepository configRepo, IReadOnlyReservationRepository readRepo, ICapacityCalculator capacity)
    {
        _configRepo = configRepo;
        _readRepo = readRepo;
        _capacity = capacity;
    }

    public async Task CheckAvailabilityAsync(CreateReservationCommand cmd, TenantContext tenant, CancellationToken ct = default)
    {
        var config = await _configRepo.GetByRestaurantAsync(cmd.RestaurantId, tenant, ct)
            ?? throw new InvalidOperationException("RestaurantConfig no encontrado");

        // Service period
        var period = config.ServicePeriods
            .Where(p => p.IsActive)
            .FirstOrDefault(p => cmd.Hora >= p.StartTime && cmd.Hora < p.EndTime);
        if (period is null)
            throw new InvalidOperationException("Horario fuera de servicio");

        // Party size rules
        if (cmd.PartySize < config.MinPartySize || cmd.PartySize > config.MaxPartySize)
            throw new InvalidOperationException("PartySize fuera de rango");

        // Anticipación
        var nowUtc = DateTime.UtcNow;
        var reservationDateTime = cmd.Fecha.Date + cmd.Hora;
        var minAllowed = nowUtc.AddMinutes(config.MinMinutesBeforeReservation);
        var maxAllowed = nowUtc.AddDays(config.MaxDaysInAdvance);
        if (reservationDateTime < minAllowed || reservationDateTime > maxAllowed)
            throw new InvalidOperationException("Anticipación inválida");

        // Capacity
        var totalCapacity = await _capacity.GetTotalCapacityAsync(cmd.RestaurantId, tenant, ct);

        // Overlaps window based on average stay
        var start = cmd.Hora;
        var end = cmd.Hora + TimeSpan.FromMinutes(config.AverageStayMinutes);
        var overlaps = await _readRepo.GetOverlappingAsync(cmd.RestaurantId, cmd.Fecha.Date, start, end, tenant, ct);
        var seated = overlaps.Sum(r => r.PartySize);

        var allowed = totalCapacity;
        if (config.AllowOverbooking)
        {
            allowed = (int)Math.Floor(totalCapacity * (1 + (config.OverbookingPercentage / 100.0)));
        }

        if (seated + cmd.PartySize > allowed)
            throw new InvalidOperationException("Capacidad excedida");

        // else: available
    }
}
