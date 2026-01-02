namespace ReserveOne.Domain.Entities;

public class Table : Entity
{
    public Guid SalonId { get; set; }
    public int Capacidad { get; set; }
}
