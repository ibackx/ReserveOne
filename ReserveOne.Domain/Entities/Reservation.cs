using ReserveOne.Domain.Enums;

namespace ReserveOne.Domain.Entities;

public class Reservation : Entity
{
    public Guid RestaurantId { get; set; }
    public Guid? SalonId { get; set; }
    public Guid? TableId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public TimeSpan Hora { get; set; }
    public int PartySize { get; set; }
    public ReservationStatus Status { get; set; }
    public ReservationSource Source { get; set; }
}
