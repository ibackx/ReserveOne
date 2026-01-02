using ReserveOne.Domain.Enums;

namespace ReserveOne.Application.Reservations;

public class CreateReservationCommand
{
    public Guid RestaurantId { get; set; }
    public Guid? SalonId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public TimeSpan Hora { get; set; }
    public int PartySize { get; set; }
    public ReservationSource Source { get; set; }
}
