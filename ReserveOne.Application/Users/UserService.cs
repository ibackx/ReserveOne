using ReserveOne.Application.Security;
using ReserveOne.Domain.Entities;
using ReserveOne.Domain.Enums;

namespace ReserveOne.Application.Users;

public class UserService
{
    private readonly IUserRepository _users;
    private readonly TenantContext _tenant;
    private readonly IPasswordHasher _hasher;

    public UserService(IUserRepository users, TenantContext tenant, IPasswordHasher hasher)
    {
        _users = users;
        _tenant = tenant;
        _hasher = hasher;
    }

    public async Task<CreateUserResult> CreateAsync(CreateUserRequest req, CancellationToken ct = default)
    {
        // Only Owner/Manager can create
        // Policy enforced at API level. Here, ensure Tenant exists.
        if (_tenant.TenantId is null)
            throw new InvalidOperationException("Operacion requiere TenantId");

        // Prevent duplicate email (global uniqueness)
        var existing = await _users.GetByEmailAsync(req.Email, ct);
        if (existing is not null)
            throw new InvalidOperationException("El email ya existe");

        var tempPassword = GenerateTempPassword();
        var user = new User
        {
            Id = Guid.NewGuid(),
            TenantId = _tenant.TenantId,
            Nombre = req.Name,
            Email = req.Email,
            PasswordHash = _hasher.Hash(tempPassword),
            Role = req.Role,
            Activo = true,
            MustChangePassword = true,
            Borrado = false,
            FechaCreacion = DateTime.UtcNow,
            UltimaFechaEdicion = DateTime.UtcNow,
            IdCreador = _tenant.UserId,
            IdUltimoEditor = _tenant.UserId,
        };
        await _users.AddAsync(user, ct);
        return new CreateUserResult { Id = user.Id, TempPassword = tempPassword };
    }

    public Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct = default)
    {
        if (_tenant.TenantId is null)
            throw new InvalidOperationException("Operacion requiere TenantId");
        return _users.GetAllByTenantAsync(_tenant.TenantId.Value, ct);
    }

    public async Task DeactivateAsync(Guid id, CancellationToken ct = default)
    {
        if (_tenant.TenantId is null)
            throw new InvalidOperationException("Operacion requiere TenantId");

        var user = await _users.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException();
        if (user.TenantId != _tenant.TenantId)
            throw new UnauthorizedAccessException("No pertenece al tenant");
        user.Activo = false;
        user.UltimaFechaEdicion = DateTime.UtcNow;
        user.IdUltimoEditor = _tenant.UserId;
        await _users.SaveChangesAsync(ct);
    }

    private static string GenerateTempPassword() => $"Tmp-{Guid.NewGuid():N}";
}
