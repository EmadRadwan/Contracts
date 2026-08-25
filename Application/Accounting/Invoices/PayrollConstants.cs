namespace Application.Accounting.Invoices;

/// <summary>
/// Shared identifiers for the monthly payroll run. These were previously repeated as
/// string literals across BatchCreatePayrollInvoices, GeneralLedgerService, the project /
/// company reports and the Power BI views, where they had to agree but nothing enforced it.
/// </summary>
public static class PayrollConstants
{
    /// <summary>
    /// The shared staff party the batch addresses its two aggregate settlement payments to
    /// (موظفي الشركة). It is deliberately NOT an employee: it has no EMPLOYEE PartyGlAccount,
    /// so a payment addressed to it can only ever be the whole-run settlement, never a
    /// per-employee payment. GetProjectPayroll, GetCompanyReport.GetPayroll and
    /// Fact_Project_Payroll all use this same marker to tell the two apart.
    /// </summary>
    public const string StaffPartyId = "276";

    /// <summary>
    /// InvoiceAttribute holding the employee's payroll payment method as it stood when the
    /// run was executed — the snapshot that decides which of the two aggregate payments the
    /// invoice belongs to. Stored per invoice rather than read from
    /// Party.PreferredPayrollPaymentMethodId at posting time, because changing an employee's
    /// preference must not retroactively re-partition a run that has already been executed.
    /// </summary>
    public const string PaymentMethodAttrName = "PAYROLL_PMT_METHOD";

    public const string CashMethod = "CASH";
    public const string BankTransferMethod = "BANK_TRANSFER";
}
