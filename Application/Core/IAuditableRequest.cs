namespace Application.Core;

/// <summary>
/// Opt-in marker for requests that should be audited even though the naming convention would skip
/// them - a sensitive report export, for instance. Commands are picked up automatically by name.
/// </summary>
public interface IAuditableRequest
{
}
