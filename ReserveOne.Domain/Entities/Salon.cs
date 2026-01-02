namespace ReserveOne.Domain.Entities;

public class Salon : Entity
{
    public Guid RestaurantId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int Prioridad { get; set; }
}
