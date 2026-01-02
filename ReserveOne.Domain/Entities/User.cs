using ReserveOne.Domain.Enums;

namespace ReserveOne.Domain.Entities;

public class User : Entity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool Activo { get; set; } = true;
    public bool MustChangePassword { get; set; } = true;
}
