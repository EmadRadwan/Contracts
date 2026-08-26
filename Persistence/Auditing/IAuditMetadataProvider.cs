namespace Persistence.Auditing;

/// <summary>
/// Supplies the "who / which request" context for audit rows.
/// <para>
/// This abstraction lives in Persistence on purpose: Persistence references only Domain, so it
/// cannot see Application.Interfaces.IUserAccessor (Application references Persistence, not the
/// other way round). The HTTP-aware implementation lives in Infrastructure.Security.
/// </para>
/// </summary>
public interface IAuditMetadataProvider
{
    /// <summary>Kill switch, so auditing can be turned off by config without a redeploy.</summary>
    bool IsEnabled { get; }

    /// <summary>Actor, written to ENTITY_AUDIT_LOG.CHANGED_BY_INFO. Null when unattributable.</summary>
    string? GetChangedByInfo();

    /// <summary>Display name of the signed-in user, or "system" outside a request.</summary>
    string? GetUserName();

    /// <summary>AspNetUsers.Id of the signed-in user. Null when unauthenticated.</summary>
    string? GetUserId();

    /// <summary>Request path, e.g. "/api/orders/reset". Null outside a request.</summary>
    string? GetRequestPath();

    /// <summary>HTTP verb of the current request. Null outside a request.</summary>
    string? GetHttpMethod();

    /// <summary>Caller IP, honouring X-Forwarded-For when behind the reverse proxy.</summary>
    string? GetClientIpAddress();

    /// <summary>
    /// Correlation key, written to ENTITY_AUDIT_LOG.CHANGED_SESSION_INFO. This is
    /// HttpContext.TraceIdentifier — the same value ExceptionMiddleware reports as the error ID
    /// and Serilog emits as {RequestId} — so a support ticket, a log line and the rows that
    /// changed can all be lined up on one key.
    /// </summary>
    string? GetCorrelationId();
}

/// <summary>
/// Fallback used by design-time tooling, tests and anything constructing a DataContext outside a
/// request. Auditing stays on so background writes are still recorded, just unattributed.
/// </summary>
public sealed class NullAuditMetadataProvider : IAuditMetadataProvider
{
    public bool IsEnabled => true;
    public string? GetChangedByInfo() => null;
    public string? GetCorrelationId() => null;
    public string? GetUserName() => null;
    public string? GetUserId() => null;
    public string? GetRequestPath() => null;
    public string? GetHttpMethod() => null;
    public string? GetClientIpAddress() => null;
}
