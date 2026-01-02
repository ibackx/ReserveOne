using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ReserveOne.Domain.Entities;

namespace ReserveOne.Infrastructure.Auditing;

public class AuditInterceptor : SaveChangesInterceptor
{
    // Stub: prepare hooks for audit
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        var context = eventData.Context;
        if (context == null) return base.SavingChanges(eventData, result);

        // TODO: populate LogEntry from ChangeTracker
        return base.SavingChanges(eventData, result);
    }
}
