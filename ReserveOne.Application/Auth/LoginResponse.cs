namespace ReserveOne.Application.Auth;

public class LoginResponse
{
    public bool RequiresPasswordChange { get; set; }
    public string? Token { get; set; }
}
