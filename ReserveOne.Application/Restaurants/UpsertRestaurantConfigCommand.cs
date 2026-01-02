using ReserveOne.Domain.Entities;

namespace ReserveOne.Application.Restaurants;

public class UpsertRestaurantConfigCommand
{
    public Guid RestaurantId { get; set; }
    public int MaxPartySize { get; set; }
    public int MinPartySize { get; set; }
    public int AverageStayMinutes { get; set; }

    public int? MaxSeatsOverride { get; set; }
    public bool AutoCalculateCapacity { get; set; }

    public bool AllowOverbooking { get; set; }
    public int OverbookingPercentage { get; set; }

    public bool RequireConfirmation { get; set; }
    public int ConfirmationMinutesBefore { get; set; }

    public int MinMinutesBeforeReservation { get; set; }
    public int MaxDaysInAdvance { get; set; }
}
