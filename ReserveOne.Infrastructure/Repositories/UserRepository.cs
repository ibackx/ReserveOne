using Microsoft.EntityFrameworkCore;
using ReserveOne.Application.Users;
using ReserveOne.Domain.Entities;

namespace ReserveOne.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ReserveOneDbContext _db;

    public UserRepository(ReserveOneDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        await _db.Users.AddAsync(user, ct);
        await _db.SaveChangesAsync(ct);
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return _db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task<IReadOnlyList<User>> GetAllByTenantAsync(Guid tenantId, CancellationToken ct = default)
    {
        return await _db.Users.Where(u => u.TenantId == tenantId).ToListAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
    {
        return _db.SaveChangesAsync(ct);
    }
}
