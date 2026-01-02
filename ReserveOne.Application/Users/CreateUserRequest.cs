using ReserveOne.Domain.Enums;

namespace ReserveOne.Application.Users;

public class CreateUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public UserRole Role { get; set; }
}
