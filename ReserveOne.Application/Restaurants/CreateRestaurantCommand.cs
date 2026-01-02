namespace ReserveOne.Application.Restaurants;

public class CreateRestaurantCommand
{
    public string Nombre { get; set; } = string.Empty;
    public string TimeZone { get; set; } = "UTC";
}
