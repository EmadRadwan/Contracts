// Add to your DTO (PartyFinancialHistoryDetails)
public class PartyFinancialHistoryDetails
{
    public string PartyId { get; set; }
    public string MainRole { get; set; }                     // ← already added
    public string LedgerPerspective { get; set; }            // ← new field: "Company" or "External"
    public string? PreferredCurrencyUomId { get; set; }
    // ... all other existing properties
    // ...
}

// Then in the handler, after retrieving the party, add this logic:

// 9. Return result
var perspective = DetermineLedgerPerspective(party.MainRole);

return Result<PartyFinancialHistoryDetails>.Success(new PartyFinancialHistoryDetails
{
    PartyId = request.PartyId,
    MainRole = party.MainRole,
    LedgerPerspective = perspective,                       // ← added
    PreferredCurrencyUomId = party.PreferredCurrencyUomId ?? request.DefaultCurrencyUomId,
    InvoicesApplPayments = invoicesApplPaymentsDtos,
    UnappliedInvoices = unappliedInvoicesDtos,
    UnappliedPayments = unappliedPaymentsDtos,
    BillingAccounts = billingAccountsDtos,
    Returns = returns,
    RentalPropertyPostings = rentalPropertyDtos,
    PartnerAccrualPostings = partnerAccrualDtos,
    OpeningBalances = openingBalanceDtos,
    FinancialSummary = financialSummary
});

// Helper method (can be private inside Handler or in a static utility class)
private static string DetermineLedgerPerspective(string? mainRole)
{
    if (string.IsNullOrWhiteSpace(mainRole))
        return "Company"; // fallback

    var role = mainRole.Trim().ToUpperInvariant();

    return role switch
    {
        "CUSTOMER"    => "External",
        "SUPPLIER"    => "External",
        "CONTRACTOR"  => "External",
        "PARTNER"     => "External",      // revenue share partners usually want their view
        "VENDOR"      => "External",
        _             => "Company"        // employees, internal, unknown roles → company view
    };
}