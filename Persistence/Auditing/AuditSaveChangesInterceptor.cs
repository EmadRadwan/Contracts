using System.Globalization;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Persistence.Auditing;

/// <summary>
/// Writes field-level change history into the existing ENTITY_AUDIT_LOG table (the OFBiz audit
/// table that was ported with the schema but never wired up).
///
/// <para>
/// Handlers in this codebase catch their own exceptions and return Result&lt;T&gt; rather than
/// throwing, so a partial or wrong write leaves no trace — that is how ResetProjectCertificate
/// orphaned certificate 233-0026's invoices with nothing in logs/. LoggingBehavior closed the
/// "handler failed" half of that gap; this closes the "what did it actually change" half.
/// </para>
///
/// <para>
/// Implemented as an EF interceptor rather than a DataContext.SaveChangesAsync override so that
/// Persistence needs no reference to Application (for IUserAccessor) and DataContext's
/// constructors — including the parameterless one used by design-time tooling — stay untouched.
/// Rows are added to the same SaveChanges call as the business data, so the change log is
/// transactional with it: a rolled-back write can never leave a phantom audit row claiming a
/// change that did not commit.
/// </para>
///
/// <para>
/// What gets recorded follows one rule: <b>log what you cannot recover from the live table.</b>
/// <list type="bullet">
/// <item>Modified — one row per genuinely changed field. The before-value is otherwise lost.</item>
/// <item>Deleted — a full snapshot, one row per field, because the row is about to be gone.</item>
/// <item>Added — a single marker row. The values are still readable in the table itself, so a
/// full column-by-column dump would be pure volume.</item>
/// </list>
/// </para>
/// </summary>
public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private const int MaxTextLength = 255; // every varchar on ENTITY_AUDIT_LOG is 255

    private readonly IAuditMetadataProvider _metadata;
    private readonly ILogger<AuditSaveChangesInterceptor>? _logger;

    public AuditSaveChangesInterceptor(
        IAuditMetadataProvider metadata,
        ILogger<AuditSaveChangesInterceptor>? logger = null)
    {
        _metadata = metadata;
        _logger = logger;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, InterceptionResult<int> result)
    {
        Capture(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Capture(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void Capture(DbContext? context)
    {
        if (context is null || !_metadata.IsEnabled) return;

        try
        {
            // If a previous SaveChanges threw and the caller is retrying on the same context, the
            // audit rows from that attempt are still tracked as Added while the business entries
            // are still Modified. Drop them so a retry re-derives the change set instead of
            // doubling it.
            foreach (var stale in context.ChangeTracker.Entries<EntityAuditLog>()
                         .Where(e => e.State == EntityState.Added).ToList())
                stale.State = EntityState.Detached;

            var entries = context.ChangeTracker.Entries()
                .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
                .Where(e => AuditedEntities.IsAudited(e.Metadata.ClrType.Name))
                .ToList();

            if (entries.Count == 0) return;

            // One timestamp for the whole SaveChanges, so rows written by the same transaction
            // share an exact CREATED_TX_STAMP and group cleanly in queries.
            var now = DateTime.UtcNow;
            var changedBy = Truncate(_metadata.GetChangedByInfo());
            var correlationId = Truncate(_metadata.GetCorrelationId());

            var rows = new List<EntityAuditLog>();

            foreach (var entry in entries)
            {
                var entityName = entry.Metadata.ClrType.Name;
                var pkText = BuildPkText(entry);

                switch (entry.State)
                {
                    case EntityState.Added:
                        rows.Add(NewRow(entityName, AuditedEntities.CreateMarker, pkText,
                            null, null, now, changedBy, correlationId));
                        break;

                    case EntityState.Deleted:
                        rows.Add(NewRow(entityName, AuditedEntities.DeleteMarker, pkText,
                            null, null, now, changedBy, correlationId));

                        foreach (var prop in entry.Properties)
                        {
                            if (AuditedEntities.IsIgnoredField(prop.Metadata.Name)) continue;

                            var removed = Format(prop.OriginalValue);
                            if (removed is null) continue; // nothing lost, skip the row

                            rows.Add(NewRow(entityName, prop.Metadata.Name, pkText,
                                removed, null, now, changedBy, correlationId));
                        }
                        break;

                    case EntityState.Modified:
                        foreach (var prop in entry.Properties)
                        {
                            if (!prop.IsModified) continue;
                            if (AuditedEntities.IsIgnoredField(prop.Metadata.Name)) continue;

                            var oldValue = Format(prop.OriginalValue);
                            var newValue = Format(prop.CurrentValue);

                            // Handlers that do `Entry(x).State = Modified` mark every property
                            // dirty whether or not it changed, so compare before emitting.
                            if (string.Equals(oldValue, newValue, StringComparison.Ordinal)) continue;

                            rows.Add(NewRow(entityName, prop.Metadata.Name, pkText,
                                oldValue, newValue, now, changedBy, correlationId));
                        }
                        break;
                }
            }

            if (rows.Count > 0) context.Set<EntityAuditLog>().AddRange(rows);
        }
        catch (Exception ex)
        {
            // Auditing must never be the reason a business transaction fails.
            _logger?.LogError(ex, "Audit capture failed; the save was allowed to proceed unaudited");
        }
    }

    private static EntityAuditLog NewRow(
        string entityName, string? fieldName, string? pkText,
        string? oldValue, string? newValue,
        DateTime now, string? changedBy, string? correlationId) => new()
    {
        AuditHistorySeqId = Guid.NewGuid().ToString(), // 36 chars, matches the column exactly
        ChangedEntityName = Truncate(entityName),
        ChangedFieldName = Truncate(fieldName),
        PkCombinedValueText = pkText,
        OldValueText = oldValue,
        NewValueText = newValue,
        ChangedDate = now,
        ChangedByInfo = changedBy,
        ChangedSessionInfo = correlationId,
        CreatedStamp = now,
        CreatedTxStamp = now,
        LastUpdatedStamp = now,
        LastUpdatedTxStamp = now
    };

    /// <summary>
    /// Builds a readable composite key, e.g. "OrderId=WS10000::OrderItemSeqId=00001".
    /// Safe for Added entities because this model assigns string IDs in application code (OFBiz
    /// heritage) rather than relying on store-generated values, so the key is already set here.
    /// </summary>
    private static string? BuildPkText(EntityEntry entry)
    {
        var key = entry.Metadata.FindPrimaryKey();
        if (key is null) return null;

        var parts = key.Properties.Select(p =>
            $"{p.Name}={Format(entry.Property(p.Name).CurrentValue) ?? "null"}");

        return Truncate(string.Join("::", parts));
    }

    private static string? Format(object? value) => value switch
    {
        null => null,
        DateTime dt => Truncate(dt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)),
        decimal dec => Truncate(dec.ToString(CultureInfo.InvariantCulture)),
        double dbl => Truncate(dbl.ToString(CultureInfo.InvariantCulture)),
        float flt => Truncate(flt.ToString(CultureInfo.InvariantCulture)),
        bool bln => bln ? "true" : "false",
        byte[] bytes => $"[{bytes.Length} bytes]",
        _ => Truncate(value.ToString())
    };

    /// <summary>Truncates explicitly rather than letting MySQL reject the insert mid-transaction.</summary>
    private static string? Truncate(string? value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= MaxTextLength ? value : value[..MaxTextLength];
    }
}
