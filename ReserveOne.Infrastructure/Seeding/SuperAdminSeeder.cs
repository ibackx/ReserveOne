using ReserveOne.Application.Security;
using ReserveOne.Application.Users;
using ReserveOne.Domain.Entities;
using ReserveOne.Domain.Enums;

namespace ReserveOne.Infrastructure.Seeding;

public static class SuperAdminSeeder
{
    public static async Task SeedAsync(ReserveOneDbContext db, IPasswordHasher hasher)
    {
        var email = "admin@reserveone.system";
        if (db.Users.Any(u => u.Email == email)) return;

        var user = new User
        {
            Id = Guid.NewGuid(),
            TenantId = null,
            Nombre = "System SuperAdmin",
            Email = email,
            PasswordHash = hasher.Hash("Consulting2025!"),
            Role = UserRole.SuperAdmin,
            Activo = true,
            MustChangePassword = false,
            Borrado = false,
            FechaCreacion = DateTime.UtcNow,
            UltimaFechaEdicion = DateTime.UtcNow,
            IdCreador = Guid.Empty,
            IdUltimoEditor = Guid.Empty,
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();
    }
}
