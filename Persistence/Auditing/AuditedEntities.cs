namespace Persistence.Auditing;

/// <summary>
/// Controls what the audit interceptor records.
/// <para>
/// This is a whitelist, deliberately. The model has ~868 entities carrying LastUpdatedStamp;
/// auditing all of them field-by-field would bury the signal and grow ENTITY_AUDIT_LOG far faster
/// than it could be read. Start with the entities that actually generate support calls and add
/// more here as needed — this is the single place to edit.
/// </para>
/// </summary>
public static class AuditedEntities
{
    /// <summary>Marker written to CHANGED_FIELD_NAME for inserts.</summary>
    public const string CreateMarker = "*CREATE*";

    /// <summary>Marker written to CHANGED_FIELD_NAME for the PK row of a delete.</summary>
    public const string DeleteMarker = "*DELETE*";

    /// <summary>CLR type names (Domain class names) that are audited.</summary>
    private static readonly HashSet<string> Audited = new(StringComparer.Ordinal)
    {
        // Order to cash
        "OrderHeader",
        "OrderItem",
        "OrderItemBilling",

        // Billing
        "Invoice",
        "InvoiceItem",

        // Money movement
        "Payment",
        "PaymentApplication",

        // General ledger
        "AcctgTran",
        "AcctgTransEntry",

        // Projects / work
        "WorkEffort",

        // Stock
        "InventoryItem",
        "InventoryItemDetail"
    };

    /// <summary>
    /// Fields never worth a row. The four OFBiz stamps change on virtually every write, so
    /// auditing them would roughly double the row count while telling you nothing you cannot read
    /// from ChangedDate. The credential patterns are a safety net: no whitelisted entity carries a
    /// secret today, but that should stay true even if the whitelist above grows.
    /// </summary>
    private static readonly HashSet<string> IgnoredFields = new(StringComparer.Ordinal)
    {
        "LastUpdatedStamp",
        "LastUpdatedTxStamp",
        "CreatedStamp",
        "CreatedTxStamp"
    };

    private static readonly string[] SensitiveFragments = { "password", "token", "secret", "apikey" };

    public static bool IsAudited(string clrTypeName) => Audited.Contains(clrTypeName);

    public static bool IsIgnoredField(string fieldName) =>
        IgnoredFields.Contains(fieldName) || IsSensitiveField(fieldName);

    /// <summary>
    /// True for field names that look like credentials. Used both to skip audit rows and to
    /// redact command payloads in AUDIT_ACTIVITY, so the two layers can never disagree about
    /// what counts as a secret.
    /// </summary>
    public static bool IsSensitiveField(string fieldName)
    {
        foreach (var fragment in SensitiveFragments)
            if (fieldName.Contains(fragment, StringComparison.OrdinalIgnoreCase))
                return true;

        return false;
    }
}
