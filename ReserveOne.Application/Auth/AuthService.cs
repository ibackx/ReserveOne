using ReserveOne.Application.Security;
using ReserveOne.Application.Users;
using ReserveOne.Domain.Entities;

namespace ReserveOne.Application.Auth;

public class AuthService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;

    public AuthService(IUserRepository users, IPasswordHasher hasher)
    {
        _users = users;
        _hasher = hasher;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest req, CancellationToken ct = default)
    {
        var user = await _users.GetByEmailAsync(req.Email, ct);
        if (user is null || !_hasher.Verify(req.Password, user.PasswordHash) || !user.Activo)
            throw new UnauthorizedAccessException("Credenciales inválidas");

        return new LoginResponse
        {
            RequiresPasswordChange = user.MustChangePassword,
            Token = null // JWT stub, se emite en API
        };
    }

    public async Task ChangePasswordAsync(User user, ChangePasswordRequest req, CancellationToken ct = default)
    {
        if (!_hasher.Verify(req.OldPassword, user.PasswordHash))
            throw new UnauthorizedAccessException("Password actual inválido");

        user.PasswordHash = _hasher.Hash(req.NewPassword);
        user.MustChangePassword = false;
        await _users.SaveChangesAsync(ct);
    }
}
