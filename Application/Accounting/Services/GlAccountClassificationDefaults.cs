using Domain;

namespace Application.Accounting.Services;

/// <summary>
/// Single source of truth for the six-level reporting classification that must be stamped on every
/// party sub-ledger GL account (customer / supplier / contractor / employee) at creation time.
///
/// WHY THIS EXISTS
///   Dim_gl_account — the view Power BI reads — only admits an account when the first FIVE levels are
///   non-null (SUBACCOUNT, the sixth, is optional). An account missing any of those five does not show
///   up as a blank row in reports; it vanishes completely and silently from every measure that filters
///   on REPORT / CLASS / ACCOUNT.
///
///   Ten separate call sites across nine handlers create these accounts, and every one of them used to
///   set GlAccountTypeId / GlAccountClassId / ParentGlAccountId but not the classification levels. The
///   result: 141 accounts (new customers, suppliers, employees created 2026-05 → 2026-08) were invisible
///   to Power BI, hiding ~185.6M EGP of movement — 176.8M of it customer collections. Duplicating six
///   assignments across ten sites is exactly how that was missed, so the mapping lives here instead.
///
/// HOW TO USE
///   Construct the GlAccount as before, then call <see cref="Apply"/> immediately after — before the
///   entity is added to the context:
///
///       var account = new GlAccount { ..., ParentGlAccountId = "121100", ... };
///       GlAccountClassificationDefaults.Apply(account);
///       _context.GlAccounts.Add(account);
///
/// ADDING A NEW PARENT
///   <see cref="Apply"/> throws if the parent is not mapped below. That is deliberate: every call site
///   passes a hardcoded parent constant, so the exception can never be triggered by user data — only by
///   a developer introducing a new parent account without registering its classification here. Failing
///   loudly at that moment is the whole point; the alternative is another silent reporting hole.
///   To add one, copy the classification from an already-correct sibling under the same parent.
///
/// NOTE ON GlAccountOrganization
///   Membership of the company (gl_account_organization) is a separate requirement that the view also
///   enforces via an inner join. Every call site already creates that row correctly — verified — so it
///   is intentionally NOT this helper's concern.
/// </summary>
public static class GlAccountClassificationDefaults
{
    /// <summary>The six reporting levels, broad → narrow, as stored on <see cref="GlAccount"/>.</summary>
    public sealed record Classification(
        string ReportId,
        string ClassCourseId,
        string SubClassId,
        string SubClass2Id,
        string AccountCourseLabelId,
        string SubAccountCourseLabelId);

    /// <summary>
    /// Parent GL account → the classification its children carry. Each value below is the one the
    /// overwhelming majority of that parent's already-classified children use:
    ///   121100 → 167 of 168 siblings, 124100 → 66 of 66, 210000 → 185 of 188, 220000 → 75 of 75.
    /// </summary>
    private static readonly Dictionary<string, Classification> ByParentGlAccountId = new()
    {
        // عملاء مشاريع تحت التنفيذ — project customers (CreateCustomer / UpdateCustomer / CreateParty)
        ["121100"] = new Classification(
            "BALANCE_SHEET", "ASSETS", "ASSETS", "CURRENT_ASSETS", "RECEIVABLES", "Project Receivables"),

        // ذمم الموظفين — employee receivables / advances (CreateEmployee / UpdateEmployee, loan account)
        ["124100"] = new Classification(
            "BALANCE_SHEET", "ASSETS", "ASSETS", "CURRENT_ASSETS", "RECEIVABLES", "Staff Receivables"),

        // الدائنون — suppliers and subcontractors (CreateSupplier / CreateContractor / Update* / CreateParty)
        ["210000"] = new Classification(
            "BALANCE_SHEET", "LIABILITIES_AND_OWNERS_EQUITY", "LIABILITIES", "CURRENT_LIABILITIES",
            "TRADE_PAYABLES", "Trade Payables"),

        // المصاريف المستحقة — accrued salaries (CreateEmployee / UpdateEmployee, accrued account)
        ["220000"] = new Classification(
            "BALANCE_SHEET", "LIABILITIES_AND_OWNERS_EQUITY", "LIABILITIES", "CURRENT_LIABILITIES",
            "OTHER_PAYABLES", "Accrued Expenses")
    };

    /// <summary>
    /// Looks up the classification registered for a parent account, without throwing.
    /// </summary>
    public static bool TryGetForParent(string? parentGlAccountId, out Classification? classification)
    {
        classification = null;
        if (string.IsNullOrWhiteSpace(parentGlAccountId)) return false;
        return ByParentGlAccountId.TryGetValue(parentGlAccountId, out classification);
    }

    /// <summary>
    /// Stamps the six reporting levels onto <paramref name="account"/> based on its ParentGlAccountId.
    /// Only fills levels that are currently null, so an explicit override set by the caller is never
    /// clobbered. Safe to call twice.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The account has no ParentGlAccountId, or its parent has no classification registered above.
    /// </exception>
    public static void Apply(GlAccount account)
    {
        ArgumentNullException.ThrowIfNull(account);

        if (string.IsNullOrWhiteSpace(account.ParentGlAccountId))
            throw new InvalidOperationException(
                $"Cannot classify GL account '{account.GlAccountId}': ParentGlAccountId is not set. " +
                "Party sub-ledger accounts must hang off a known parent so their reporting " +
                "classification can be derived.");

        if (!ByParentGlAccountId.TryGetValue(account.ParentGlAccountId, out var c))
            throw new InvalidOperationException(
                $"No reporting classification is registered for parent GL account " +
                $"'{account.ParentGlAccountId}' (creating account '{account.GlAccountId}'). " +
                $"Add it to {nameof(GlAccountClassificationDefaults)}.{nameof(ByParentGlAccountId)}, " +
                "copying the values from an already-classified sibling under the same parent. " +
                "Without this the account is invisible to Dim_gl_account and every Power BI measure.");

        account.GlReportId ??= c.ReportId;
        account.GlClassCourseId ??= c.ClassCourseId;
        account.GlSubClassId ??= c.SubClassId;
        account.GlSubClass2Id ??= c.SubClass2Id;
        account.GlAccountCourseLabelId ??= c.AccountCourseLabelId;
        account.GlSubAccountCourseLabelId ??= c.SubAccountCourseLabelId;
    }
}
