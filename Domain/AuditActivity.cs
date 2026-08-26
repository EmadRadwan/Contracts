namespace Domain;

/// <summary>
/// One row per command execution: who asked for what, with which input, and what came back.
///
/// <para>
/// This is the "what was the user doing" layer. Its counterpart, ENTITY_AUDIT_LOG, records the
/// resulting field-level changes; the two join on <see cref="CorrelationId"/> (written there as
/// CHANGED_SESSION_INFO), which is also HttpContext.TraceIdentifier — the error ID
/// ExceptionMiddleware returns and the {RequestId} Serilog logs.
/// </para>
///
/// <para>
/// Unlike ENTITY_AUDIT_LOG rows, these are written outside the business transaction, so a command
/// that failed and rolled back still leaves a record. That is the whole point: the failures are
/// what you need to see.
/// </para>
///
/// <para>
/// No snake_case JsonObject attribute here, matching its sibling EntityAuditLog — the two are read
/// together and inconsistent serialization between them would be worse than either convention.
/// </para>
/// </summary>
public class AuditActivity
{
    public string ActivityId { get; set; } = null!;

    /// <summary>HttpContext.TraceIdentifier. Joins to ENTITY_AUDIT_LOG.CHANGED_SESSION_INFO.</summary>
    public string? CorrelationId { get; set; }

    /// <summary>Display name at the time of the action.</summary>
    public string? UserName { get; set; }

    /// <summary>AspNetUsers.Id — stable even if the display name later changes.</summary>
    public string? UserId { get; set; }

    /// <summary>Feature-qualified request name, e.g. "ResetProjectCertificate.Command".</summary>
    public string? RequestName { get; set; }

    public string? RequestPath { get; set; }
    public string? HttpMethod { get; set; }
    public string? ClientIpAddress { get; set; }

    /// <summary>Serialized command payload, with credential-ish fields redacted.</summary>
    public string? RequestJson { get; set; }

    /// <summary>False when the handler returned a failed Result or threw.</summary>
    public bool IsSuccess { get; set; }

    /// <summary>Result.Error, or the exception message when one escaped the handler.</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Exception type name when the handler threw; null for a clean failed Result.</summary>
    public string? ExceptionType { get; set; }

    public int? DurationMs { get; set; }

    /// <summary>When the handler started, UTC.</summary>
    public DateTime StartedAt { get; set; }

    public DateTime? CreatedStamp { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
}
