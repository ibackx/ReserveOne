namespace ReserveOne.Application.Users;

public class CreateUserResult
{
    public Guid Id { get; set; }
    public string TempPassword { get; set; } = string.Empty;
}
