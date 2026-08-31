using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Infrastructure.Auditing;

/// <summary>
/// Trims the two audit tables on a daily schedule.
///
/// <para>
/// Without this the audit feature is a slow leak: ENTITY_AUDIT_LOG in particular grows with every
/// field change on every whitelisted entity, forever. Adding a purge once the table holds tens of
/// millions of rows is considerably more painful than having one from the start, which is why this
/// exists before auditing is switched on in production rather than after.
/// </para>
///
/// <para>
/// Deletes run as batched raw SQL. That is deliberate on three counts: it does not load rows into
/// the ChangeTracker (a tracked delete of a million rows would exhaust memory), it does not
/// re-enter the audit interceptor, and <c>LIMIT</c> keeps each statement's lock window short so a
/// purge cannot stall live traffic.
/// </para>
/// </summary>
public class AuditRetentionService : BackgroundService
{
    // Long enough for the app to finish starting and serve traffic before any purge work begins.
    private static readonly TimeSpan StartupDelay = TimeSpan.FromMinutes(5);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AuditRetentionService> _logger;
    private readonly AuditRetentionOptions _options;

    public AuditRetentionService(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<AuditRetentionService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = AuditRetentionOptions.FromConfiguration(configuration);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Audit retention is disabled; no purge will run");
            return;
        }

        _logger.LogInformation(
            "Audit retention active: AUDIT_ACTIVITY {ActivityDays}d, ENTITY_AUDIT_LOG {ChangeLogDays}d, " +
            "daily at {Hour:00}:00 UTC, {BatchSize} rows per batch",
            _options.ActivityDays, _options.ChangeLogDays, _options.RunAtHourUtc, _options.BatchSize);

        // Run shortly after startup as well as on the daily schedule. A container that restarts
        // more often than once a day would otherwise never reach the scheduled hour.
        try
        {
            await Task.Delay(StartupDelay, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            await PurgeAsync(stoppingToken);

            var delay = TimeUntilNextRun(DateTime.UtcNow);
            _logger.LogInformation("Next audit purge in {Hours:F1} hours", delay.TotalHours);

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }

    /// <summary>Time from <paramref name="now"/> until the next occurrence of the configured hour.</summary>
    internal TimeSpan TimeUntilNextRun(DateTime now)
    {
        var next = new DateTime(now.Year, now.Month, now.Day, _options.RunAtHourUtc, 0, 0, DateTimeKind.Utc);
        if (next <= now) next = next.AddDays(1);
        return next - now;
    }

    private async Task PurgeAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DataContext>();

            var activity = await PurgeTableAsync(
                context, "AUDIT_ACTIVITY", "STARTED_AT", _options.ActivityDays, cancellationToken);

            var changes = await PurgeTableAsync(
                context, "ENTITY_AUDIT_LOG", "CHANGED_DATE", _options.ChangeLogDays, cancellationToken);

            if (activity > 0 || changes > 0)
            {
                _logger.LogInformation(
                    "Audit purge removed {ActivityRows} AUDIT_ACTIVITY and {ChangeRows} ENTITY_AUDIT_LOG rows",
                    activity, changes);
            }
            else
            {
                _logger.LogInformation("Audit purge found nothing older than the retention window");
            }
        }
        catch (OperationCanceledException)
        {
            // Shutting down; not an error.
        }
        catch (Exception ex)
        {
            // A failed purge must never take the host down with it. Worst case the tables keep
            // growing until the next run, which is recoverable; a crash loop is not.
            _logger.LogError(ex, "Audit purge failed; it will be retried on the next schedule");
        }
    }

    private async Task<int> PurgeTableAsync(
        DataContext context, string table, string dateColumn, int keepDays, CancellationToken cancellationToken)
    {
        // Zero or negative means "keep forever". This guard is the reason a mistyped config value
        // cannot empty an audit table.
        if (keepDays <= 0)
        {
            _logger.LogInformation("Retention for {Table} is unlimited; skipping", table);
            return 0;
        }

        var cutoff = DateTime.UtcNow.AddDays(-keepDays);
        var total = 0;

        // BatchSize is clamped to 100..100000 when the options are built, so it is safe to inline;
        // the cutoff stays a real parameter.
        var sql = $"DELETE FROM `{table}` WHERE `{dateColumn}` < {{0}} LIMIT {_options.BatchSize}";

        while (!cancellationToken.IsCancellationRequested)
        {
            var deleted = await context.Database.ExecuteSqlRawAsync(
                sql, new object[] { cutoff }, cancellationToken);

            total += deleted;

            if (deleted < _options.BatchSize) break;

            if (_options.PauseBetweenBatchesMs > 0)
                await Task.Delay(_options.PauseBetweenBatchesMs, cancellationToken);
        }

        if (total > 0)
            _logger.LogInformation("Purged {Rows} rows from {Table} older than {Cutoff:yyyy-MM-dd}",
                total, table, cutoff);

        return total;
    }
}
