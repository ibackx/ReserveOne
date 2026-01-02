namespace ReserveOne.Application.Restaurants;

public class CreateSalonCommand
{
    public Guid RestaurantId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int Prioridad { get; set; }
}
