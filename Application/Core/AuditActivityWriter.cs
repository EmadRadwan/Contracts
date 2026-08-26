using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Core;

/// <summary>
/// Writes AUDIT_ACTIVITY rows on a short-lived DataContext of its own.
///
/// <para>
/// This separation is the single most important detail of the activity layer. If the row were
/// added to the request's DataContext, a command that failed and rolled back would take its own
/// audit row down with it - losing exactly the record worth having. A command that succeeds and
/// one that blew up halfway must both leave a trace, so the write cannot share their fate.
/// </para>
///
/// <para>
/// The context is built from the DI-registered DbContextOptions rather than through a
/// DbContextFactory, so no second registration is needed and the existing MySQL/interceptor
/// configuration is reused as-is. AuditActivity is not in the audited-entity whitelist, so this
/// save cannot re-enter the change-log interceptor.
/// </para>
/// </summary>
public class AuditActivityWriter : IAuditActivityWriter
{
    private readonly DbContextOptions<DataContext> _options;
    private readonly ILogger<AuditActivityWriter>? _logger;

    public AuditActivityWriter(
        DbContextOptions<DataContext> options,
        ILogger<AuditActivityWriter>? logger = null)
    {
        _options = options;
        _logger = logger;
    }

    public async Task WriteAsync(AuditActivity activity, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = new DataContext(_options);
            context.AuditActivities.Add(activity);

            // Deliberately not passing the request's CancellationToken: when a client disconnects
            // mid-request the audit row is still worth writing, and that is often the case most
            // worth explaining later.
            await context.SaveChangesAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            // Auditing must never turn into an outage. Losing a row is survivable; failing the
            // request because the audit write failed is not.
            _logger?.LogError(ex,
                "Failed to write audit activity for {RequestName} (correlation {CorrelationId})",
                activity.RequestName, activity.CorrelationId);
        }
    }
}
