namespace ReserveOne.Domain.Entities;

public abstract class Entity
{
    public Guid Id { get; set; }
    public Guid? TenantId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool Borrado { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime UltimaFechaEdicion { get; set; }
    public Guid IdCreador { get; set; }
    public Guid IdUltimoEditor { get; set; }
}
