namespace ReserveOne.Domain.Entities;

public class Tenant
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public bool Borrado { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime UltimaFechaEdicion { get; set; }
    public Guid IdCreador { get; set; }
    public Guid IdUltimoEditor { get; set; }
}
