using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ReserveOne.Infrastructure;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ReserveOneDbContext>
{
    public ReserveOneDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ReserveOneDbContext>();
        optionsBuilder.UseSqlServer("Server=localhost;Database=ReserveOneDb;Trusted_Connection=True;TrustServerCertificate=True");
        return new ReserveOneDbContext(optionsBuilder.Options);
    }
}
