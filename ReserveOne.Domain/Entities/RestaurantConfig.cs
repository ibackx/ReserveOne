namespace ReserveOne.Domain.Entities;

public class RestaurantConfig : Entity
{
    public Guid RestaurantId { get; set; }

    // Estado y versionado
    public bool IsActive { get; set; }
    public int Version { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid CreatedBy { get; set; }

    // Capacidad
    public int? MaxSeatsOverride { get; set; }
    public bool AutoCalculateCapacity { get; set; }

    // Turnos / sittings
    public List<ServicePeriod> ServicePeriods { get; set; } = new();

    // Reglas de reserva
    public int MaxPartySize { get; set; }
    public int MinPartySize { get; set; }
    public int AverageStayMinutes { get; set; }

    // Overbooking
    public bool AllowOverbooking { get; set; }
    public int OverbookingPercentage { get; set; }

    // No-shows
    public bool RequireConfirmation { get; set; }
    public int ConfirmationMinutesBefore { get; set; }

    // Anticipación
    public int MinMinutesBeforeReservation { get; set; }
    public int MaxDaysInAdvance { get; set; }
}
