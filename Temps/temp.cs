var partnerAccrualDtos = new List<PartnerAccrualPostingDto>();

// ... fetch partnerAccrualEntries (as shown in previous messages)

// Then in the final return:
return Result<PartyFinancialHistoryDetails>.Success(new PartyFinancialHistoryDetails
{
    PartyId = request.PartyId,
    PreferredCurrencyUomId = party.PreferredCurrencyUomId ?? request.DefaultCurrencyUomId,
    InvoicesApplPayments = invoicesApplPaymentsDtos,
    UnappliedInvoices = unappliedInvoicesDtos,
    UnappliedPayments = unappliedPaymentsDtos,
    BillingAccounts = billingAccountsDtos,
    Returns = returns,
    OpeningBalances = openingBalanceDtos,
    RentalPropertyPostings = rentalPropertyDtos,
    PartnerAccrualPostings = partnerAccrualDtos,          // ← new
    FinancialSummary = financialSummary
});