namespace ReserveOne.Domain.Entities;

public class ServicePeriod : Entity
{
    public Guid RestaurantConfigId { get; set; }

    public string Name { get; set; } = string.Empty; // "Almuerzo", "Cena"
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }

    public bool IsActive { get; set; }
}
