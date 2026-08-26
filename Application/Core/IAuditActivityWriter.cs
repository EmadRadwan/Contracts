using Domain;

namespace Application.Core;

/// <summary>
/// Persists command-level audit rows independently of the business transaction.
/// </summary>
public interface IAuditActivityWriter
{
    Task WriteAsync(AuditActivity activity, CancellationToken cancellationToken = default);
}
