using Microsoft.Extensions.Configuration;

namespace Infrastructure.Auditing;

/// <summary>
/// Retention settings for the two audit tables, read from the "Auditing:Retention" section.
///
/// <para>
/// Deliberately independent of "Auditing:Enabled": capture can be switched off while an existing
/// backlog still needs trimming, so the purge has its own switch.
/// </para>
/// </summary>
public class AuditRetentionOptions
{
    /// <summary>Master switch for the purge. Defaults to on.</summary>
    public bool Enabled { get; init; } = true;

    /// <summary>Days of AUDIT_ACTIVITY to keep. Zero or less means keep forever.</summary>
    public int ActivityDays { get; init; } = 365;

    /// <summary>Days of ENTITY_AUDIT_LOG to keep. Zero or less means keep forever.</summary>
    public int ChangeLogDays { get; init; } = 730;

    /// <summary>UTC hour of day to run the purge (0-23).</summary>
    public int RunAtHourUtc { get; init; } = 2;

    /// <summary>Rows deleted per statement, to keep locks short on a busy table.</summary>
    public int BatchSize { get; init; } = 5000;

    /// <summary>Pause between batches, giving other traffic room to breathe.</summary>
    public int PauseBetweenBatchesMs { get; init; } = 250;

    /// <summary>
    /// Read via the configuration indexer rather than the ConfigurationBinder extensions, matching
    /// how the rest of Infrastructure reads settings. Any malformed value falls back to its
    /// default rather than throwing at startup.
    /// </summary>
    public static AuditRetentionOptions FromConfiguration(IConfiguration config)
    {
        var d = new AuditRetentionOptions();

        return new AuditRetentionOptions
        {
            Enabled = Bool(config["Auditing:Retention:Enabled"], d.Enabled),
            ActivityDays = Int(config["Auditing:Retention:ActivityDays"], d.ActivityDays),
            ChangeLogDays = Int(config["Auditing:Retention:ChangeLogDays"], d.ChangeLogDays),
            RunAtHourUtc = Clamp(Int(config["Auditing:Retention:RunAtHourUtc"], d.RunAtHourUtc), 0, 23),
            BatchSize = Clamp(Int(config["Auditing:Retention:BatchSize"], d.BatchSize), 100, 100_000),
            PauseBetweenBatchesMs = Clamp(Int(config["Auditing:Retention:PauseBetweenBatchesMs"],
                d.PauseBetweenBatchesMs), 0, 10_000)
        };
    }

    private static bool Bool(string? raw, bool fallback) =>
        bool.TryParse(raw, out var v) ? v : fallback;

    private static int Int(string? raw, int fallback) =>
        int.TryParse(raw, out var v) ? v : fallback;

    private static int Clamp(int value, int min, int max) =>
        value < min ? min : value > max ? max : value;
}
