namespace ReserveOne.Application.Restaurants;

public class CreateServicePeriodRequest
{
    public Guid RestaurantConfigId { get; set; }
    public string Name { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsActive { get; set; } = true;
}
