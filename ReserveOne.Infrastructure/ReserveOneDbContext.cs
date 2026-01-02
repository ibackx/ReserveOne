using Microsoft.EntityFrameworkCore;
using ReserveOne.Domain.Entities;

namespace ReserveOne.Infrastructure;

public class ReserveOneDbContext : DbContext
{
    public ReserveOneDbContext(DbContextOptions<ReserveOneDbContext> options) : base(options) { }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Restaurant> Restaurants => Set<Restaurant>();
    public DbSet<Salon> Salons => Set<Salon>();
    public DbSet<Table> Tables => Set<Table>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<LogEntry> LogEntries => Set<LogEntry>();
    public DbSet<RestaurantConfig> RestaurantConfigs => Set<RestaurantConfig>();
    public DbSet<ServicePeriod> ServicePeriods => Set<ServicePeriod>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Ignorar Entity base
        modelBuilder.Ignore<Entity>();

        // Soft delete global filter
        modelBuilder.Entity<Tenant>().HasQueryFilter(e => !e.Borrado);
        modelBuilder.Entity<User>().HasQueryFilter(e => !e.Borrado);
        modelBuilder.Entity<Restaurant>().HasQueryFilter(e => !e.Borrado);
        modelBuilder.Entity<Salon>().HasQueryFilter(e => !e.Borrado);
        modelBuilder.Entity<Table>().HasQueryFilter(e => !e.Borrado);
        modelBuilder.Entity<Reservation>().HasQueryFilter(e => !e.Borrado);
        modelBuilder.Entity<RestaurantConfig>().HasQueryFilter(e => !e.Borrado);
        modelBuilder.Entity<ServicePeriod>().HasQueryFilter(e => !e.Borrado);

        // TenantId required for all business entities except User (SuperAdmin has null TenantId)
        modelBuilder.Entity<Restaurant>().Property(e => e.TenantId).IsRequired();
        modelBuilder.Entity<Salon>().Property(e => e.TenantId).IsRequired();
        modelBuilder.Entity<Table>().Property(e => e.TenantId).IsRequired();
        modelBuilder.Entity<Reservation>().Property(e => e.TenantId).IsRequired();
        modelBuilder.Entity<RestaurantConfig>().Property(e => e.TenantId).IsRequired();
        modelBuilder.Entity<ServicePeriod>().Property(e => e.TenantId).IsRequired();
        // User.TenantId is optional by design (SuperAdmin)

        // Unique constraint on User.Email to prevent duplicates
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Unique active configuration per restaurant
        modelBuilder.Entity<RestaurantConfig>()
            .HasIndex(rc => new { rc.RestaurantId, rc.IsActive })
            .HasFilter("[IsActive] = 1")
            .IsUnique();

        // Business uniqueness rules
        modelBuilder.Entity<Restaurant>()
            .HasIndex(r => new { r.TenantId, r.Nombre })
            .IsUnique();
        modelBuilder.Entity<Salon>()
            .HasIndex(s => new { s.RestaurantId, s.Nombre })
            .IsUnique();
        modelBuilder.Entity<Table>()
            .HasIndex(t => new { t.SalonId, t.Nombre })
            .IsUnique();
        modelBuilder.Entity<Tenant>()
            .HasIndex(t => t.ApiKey)
            .IsUnique();

        base.OnModelCreating(modelBuilder);
    }
}
