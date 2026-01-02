using Microsoft.EntityFrameworkCore;
using ReserveOne.Application.Restaurants;
using ReserveOne.Domain.Entities;

namespace ReserveOne.Infrastructure.Repositories;

public class RestaurantConfigVersioningService
{
    private readonly ReserveOneDbContext _db;

    public RestaurantConfigVersioningService(ReserveOneDbContext db)
    {
        _db = db;
    }

    public async Task<RestaurantConfig> UpsertAsync(UpsertRestaurantConfigCommand cmd, Guid userId, Guid tenantId, CancellationToken ct = default)
    {
        using var tx = await _db.Database.BeginTransactionAsync(ct);

        var current = await _db.RestaurantConfigs
            .FirstOrDefaultAsync(rc => rc.RestaurantId == cmd.RestaurantId && rc.TenantId == tenantId && rc.IsActive, ct);

        if (current is not null)
        {
            current.IsActive = false;
            current.UltimaFechaEdicion = DateTime.UtcNow;
            current.IdUltimoEditor = userId;
            await _db.SaveChangesAsync(ct);
        }

        var version = (current?.Version ?? 0) + 1;
        var newConfig = new RestaurantConfig
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RestaurantId = cmd.RestaurantId,
            IsActive = true,
            Version = version,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId,
            MaxPartySize = cmd.MaxPartySize,
            MinPartySize = cmd.MinPartySize,
            AverageStayMinutes = cmd.AverageStayMinutes,
            MaxSeatsOverride = cmd.MaxSeatsOverride,
            AutoCalculateCapacity = cmd.AutoCalculateCapacity,
            AllowOverbooking = cmd.AllowOverbooking,
            OverbookingPercentage = cmd.OverbookingPercentage,
            RequireConfirmation = cmd.RequireConfirmation,
            ConfirmationMinutesBefore = cmd.ConfirmationMinutesBefore,
            MinMinutesBeforeReservation = cmd.MinMinutesBeforeReservation,
            MaxDaysInAdvance = cmd.MaxDaysInAdvance,
            Borrado = false,
            FechaCreacion = DateTime.UtcNow,
            UltimaFechaEdicion = DateTime.UtcNow,
            IdCreador = userId,
            IdUltimoEditor = userId,
        };

        await _db.RestaurantConfigs.AddAsync(newConfig, ct);
        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return newConfig;
    }
}
