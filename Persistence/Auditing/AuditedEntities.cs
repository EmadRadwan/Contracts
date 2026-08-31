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

        // Commissions. Added after a dev test showed UpdateSalesCommission rewriting a
        // commission's nets with no field-level trace — the exact class of problem this
        // feature exists to explain.
        "SalesCommission",
        "SalesRequest",

        // Stock
        "InventoryItem",
        "InventoryItemDetail",

        // -------------------------------------------------------------------------------
        // Master and reference data.
        //
        // Everything above records what happened. The entries below record changes to the
        // settings that decide how it is interpreted — and in several past incidents a field
        // edited today silently changed what an already-closed period reported.
        // -------------------------------------------------------------------------------

        // Party carries two fields with outsized reach:
        //   PreferredPayrollPaymentMethodId — flipping it retroactively removed an employee
        //     from an already-executed payroll run, so payment 16855 could no longer post
        //     (322,078 against a re-derived run total of 262,745).
        //   GlAccountIdAdvancedPayment — decides which GL account payroll debits, and is why
        //     a large share of payroll never reaches the P&L.
        "Party",

        // The accrued ACCOUNTS_PAYABLE that payroll posting resolves against.
        "PartyGlAccount",

        // GL classification drives the trial balance, every report and all of Power BI, and is
        // by far the most hand-corrected area in API/Sql.
        // NOTE: those corrections are run as raw SQL, which bypasses the ChangeTracker
        // entirely — this captures edits made through the application only.
        "GlAccount",
        "GlSubClass",
        "GlSubClass2",
        "GlAccountCourseLabel",

        // Six rows, rarely touched — but opening or closing a period changes what every
        // balance inside it includes.
        "CustomTimePeriod",

        // Holds the PAYROLL_PMT_METHOD snapshot (commit 130429d1) that exists specifically to
        // stop payroll history being rewritten. Worth auditing the safety mechanism itself.
        "InvoiceAttribute"
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
