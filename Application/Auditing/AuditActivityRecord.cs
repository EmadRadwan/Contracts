using System.ComponentModel.DataAnnotations;

namespace Application.Auditing;

/// <summary>
/// OData projection of AUDIT_ACTIVITY — the "who did what" trail.
/// Joins to <see cref="EntityAuditLogRecord.ChangedSessionInfo"/> on <see cref="CorrelationId"/>.
/// </summary>
public class AuditActivityRecord
{
    [Key] public string ActivityId { get; set; } = null!;

    public DateTime StartedAt { get; set; }
    public string? UserName { get; set; }
    public string? UserId { get; set; }
    public string? RequestName { get; set; }
    public string? RequestPath { get; set; }
    public string? HttpMethod { get; set; }
    public string? ClientIpAddress { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ExceptionType { get; set; }
    public int? DurationMs { get; set; }

    /// <summary>HttpContext.TraceIdentifier — the key that ties an action to the rows it changed.</summary>
    public string? CorrelationId { get; set; }

    /// <summary>Redacted command payload. Large; use $select to omit it from grid queries.</summary>
    public string? RequestJson { get; set; }
}
