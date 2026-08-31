using System.ComponentModel.DataAnnotations;

namespace Application.Auditing;

/// <summary>
/// OData projection of ENTITY_AUDIT_LOG — the "what actually changed" trail.
/// One row per changed field, plus *CREATE* / *DELETE* markers.
/// </summary>
public class EntityAuditLogRecord
{
    [Key] public string AuditHistorySeqId { get; set; } = null!;

    public string? ChangedEntityName { get; set; }

    /// <summary>Field name, or the *CREATE* / *DELETE* marker.</summary>
    public string? ChangedFieldName { get; set; }

    /// <summary>Readable composite key, e.g. "PaymentId=O17827".</summary>
    public string? PkCombinedValueText { get; set; }

    public string? OldValueText { get; set; }
    public string? NewValueText { get; set; }
    public DateTime? ChangedDate { get; set; }

    /// <summary>"name (AspNetUsers.Id)" at the time of the change.</summary>
    public string? ChangedByInfo { get; set; }

    /// <summary>Correlation id; joins to <see cref="AuditActivityRecord.CorrelationId"/>.</summary>
    public string? ChangedSessionInfo { get; set; }
}
