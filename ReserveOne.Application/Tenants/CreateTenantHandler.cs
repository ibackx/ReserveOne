using ReserveOne.Application.Security;
using ReserveOne.Application.Users;
using ReserveOne.Domain.Entities;
using ReserveOne.Domain.Enums;

namespace ReserveOne.Application.Tenants;

public class CreateTenantHandler
{
    private readonly ITenantRepository _tenantRepo;
    private readonly IUserRepository _userRepo;
    private readonly IPasswordHasher _hasher;
    private readonly TenantContext _context;

    public CreateTenantHandler(ITenantRepository tenantRepo, IUserRepository userRepo, IPasswordHasher hasher, TenantContext context)
    {
        _tenantRepo = tenantRepo;
        _userRepo = userRepo;
        _hasher = hasher;
        _context = context;
    }

    public async Task<CreateTenantResult> HandleAsync(CreateTenantCommand cmd, CancellationToken ct = default)
    {
        // Policy: only SuperAdmin
        if (_context.TenantId != null)
            throw new UnauthorizedAccessException("Solo SuperAdmin");

        var apiKey = GenerateApiKey();
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Nombre = cmd.TenantName,
            ApiKey = apiKey,
            Borrado = false,
            FechaCreacion = DateTime.UtcNow,
            UltimaFechaEdicion = DateTime.UtcNow,
            IdCreador = _context.UserId,
            IdUltimoEditor = _context.UserId,
        };

        await _tenantRepo.AddAsync(tenant, ct);

        // create admin user with temp password
        var tempPassword = GenerateTempPassword();
        var admin = new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            Nombre = cmd.AdminName,
            Email = cmd.AdminEmail,
            PasswordHash = _hasher.Hash(tempPassword),
            Role = UserRole.Owner,
            Activo = true,
            MustChangePassword = true,
            Borrado = false,
            FechaCreacion = DateTime.UtcNow,
            UltimaFechaEdicion = DateTime.UtcNow,
            IdCreador = _context.UserId,
            IdUltimoEditor = _context.UserId,
        };
        await _userRepo.AddAsync(admin, ct);

        return new CreateTenantResult
        {
            TenantId = tenant.Id,
            AdminUserId = admin.Id,
            AdminTempPassword = tempPassword
        };
    }

    private static string GenerateTempPassword() => $"Tmp-{Guid.NewGuid():N}";

    private static string GenerateApiKey() => Convert.ToBase64String(Guid.NewGuid().ToByteArray())
        .Replace("+", "").Replace("/", "").Replace("=", "");
}
