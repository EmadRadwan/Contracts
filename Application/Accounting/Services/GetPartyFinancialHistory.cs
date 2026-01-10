using Application.Accounting.Services.Models;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.Services;

public class GetPartyFinancialHistory
{
    public class Query : IRequest<Result<PartyFinancialHistoryDetails>>
    {
        public string PartyId { get; set; }
        public string? OrganizationPartyId { get; set; } // Optional
        public string? DefaultCurrencyUomId { get; set; } // Optional
    }

    public class Handler : IRequestHandler<Query, Result<PartyFinancialHistoryDetails>>
    {
        private readonly DataContext _context;
        private readonly IInvoiceUtilityService _invoiceUtilityService;
        private readonly IPaymentWorkerService _paymentWorkerService;
        private readonly IBillingAccountService _billingAccountService;
        private readonly IUserAccessor _userAccessor;
        private readonly IAcctgMiscService _acctgMiscService;


        public Handler(
            DataContext context,
            IInvoiceUtilityService invoiceUtilityService,
            IPaymentWorkerService paymentWorkerService,
            IBillingAccountService billingAccountService,
            IUserAccessor userAccessor,
            IAcctgMiscService acctgMiscService
        )
        {
            _context = context;
            _invoiceUtilityService = invoiceUtilityService;
            _paymentWorkerService = paymentWorkerService;
            _billingAccountService = billingAccountService;
            _userAccessor = userAccessor;
            _acctgMiscService = acctgMiscService;
        }

        public async Task<Result<PartyFinancialHistoryDetails>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            try
            {
                // 1. Retrieve Party
                var party = await _context.Parties
                    .FirstOrDefaultAsync(p => p.PartyId == request.PartyId, cancellationToken);

                if (party == null)
                {
                    return Result<PartyFinancialHistoryDetails>.Failure("Party not found.");
                }

                string organizationPartyId = request.OrganizationPartyId;
                if (string.IsNullOrWhiteSpace(organizationPartyId))
                {
                    var user = await _context.Users
                        .SingleOrDefaultAsync(x => x.UserName == _userAccessor.GetUsername(), cancellationToken);

                    var userLogin =
                        await _context.UserLogins.SingleOrDefaultAsync(x => x.UserLoginId == user.UserLoginId);

                    if (user == null)
                    {
                        return Result<PartyFinancialHistoryDetails>.Failure("User not found.");
                    }


                    organizationPartyId = user.OrganizationPartyId;
                }


                // 2. Determine currency for calculations
                // Business Purpose: Establish the currency to be used for financial calculations, prioritizing the requested currency,
                // then the party's preferred currency, and finally the organization's base currency.
                // This ensures consistent currency handling across all financial data retrieved.

                string currencyUomId = request.DefaultCurrencyUomId;
                if (string.IsNullOrWhiteSpace(currencyUomId))
                {
                    currencyUomId = party.PreferredCurrencyUomId;
                    if (string.IsNullOrWhiteSpace(currencyUomId))
                    {
                        var partyAccountingPreferences =
                            await _acctgMiscService.GetPartyAccountingPreferences(organizationPartyId);
                        if (partyAccountingPreferences == null)
                        {
                            return Result<PartyFinancialHistoryDetails>.Failure(
                                "Party accounting preferences not found.");
                        }

                        currencyUomId = partyAccountingPreferences.BaseCurrencyUomId;
                        if (string.IsNullOrWhiteSpace(currencyUomId))
                        {
                            return Result<PartyFinancialHistoryDetails>.Failure(
                                "Base currency not found in accounting preferences.");
                        }
                    }
                }


                // SPECIAL REMARK: Determine currency for calculations
                // Business Purpose: Check if the actual currency differs from the party's preferred currency to handle conversions appropriately.
                // This flag ensures accurate financial reporting when currency conversion is required.

                bool actualCurrency = !string.IsNullOrEmpty(party.PreferredCurrencyUomId) &&
                                      party.PreferredCurrencyUomId != currencyUomId;

                var openingBalanceEntries = await _context.AcctgTransEntries
                    .Where(ate =>
                        ate.AcctgTrans.AcctgTransTypeId == "OPENING_BALANCE" &&
                        ate.AcctgTrans.PartyId == request.PartyId &&
                        ate.AcctgTrans.IsPosted == "Y")
                    .Select(ate => new
                    {
                        ate.AcctgTrans.TransactionDate,
                        ate.GlAccountTypeId, // Critical: tells us if it's AR or AP
                        ate.DebitCreditFlag, // "D" = Debit, "C" = Credit
                        ate.Amount,
                        // Use actual currency if available, fallback to orig or default
                        CurrencyUomId = !string.IsNullOrEmpty(ate.CurrencyUomId)
                            ? ate.CurrencyUomId
                            : (!string.IsNullOrEmpty(ate.OrigCurrencyUomId)
                                ? ate.OrigCurrencyUomId
                                : currencyUomId)
                    })
                    .ToListAsync(cancellationToken);

                bool IsReceivableAccountType(string glAccountTypeId)
                {
                    return glAccountTypeId switch
                    {
                        "ACCOUNTS_RECEIVABLE" => true,
                        "ACCREC_UNAPPLIED" => true,
                        "MRCH_STLMNT_ACCOUNT" => true, // Merchant settlement is still receivable
                        "INTRSTINC_RECEIVABLE" => true, // Interest receivable
                        _ => false // Everything else (AP, deposits, etc.) = liability
                    };
                }

                decimal openingBalanceImpact = 0m; // This will be added to final net amount
                var openingBalanceDtos = new List<OpeningBalanceDto>();

                foreach (var entry in openingBalanceEntries)
                {
                    // Standard accrual accounting:
                    // Debit to AR  → increases what customer owes us → positive
                    // Credit to AR → reduces receivable → negative
                    decimal signedAmount = (decimal)(entry.DebitCreditFlag == "D"
                        ? entry.Amount
                        : -entry.Amount);

                    bool isReceivable = IsReceivableAccountType(entry.GlAccountTypeId);

                    // Final impact on "how much customer owes us":
                    // - If posted to AR → use normal sign
                    // - If posted to AP or Customer Deposit → reverse the sign (liability)
                    decimal impactOnCustomerBalance = isReceivable
                        ? signedAmount
                        : -signedAmount;

                    openingBalanceImpact += Math.Round(impactOnCustomerBalance, 2, MidpointRounding.AwayFromZero);

                    // Optional: expose details in response (great for audit/troubleshooting)
                    openingBalanceDtos.Add(new OpeningBalanceDto
                    {
                        TransactionDate = entry.TransactionDate,
                        GlAccountTypeId = entry.GlAccountTypeId,
                        Amount = (decimal)entry.Amount,
                        DebitCreditFlag = entry.DebitCreditFlag,
                        CurrencyUomId = entry.CurrencyUomId,
                        ImpactOnBalance = Math.Round(impactOnCustomerBalance, 2, MidpointRounding.AwayFromZero),
                        Description = "Opening Balance"
                    });
                }

                openingBalanceDtos = openingBalanceDtos
                    .OrderBy(x => x.TransactionDate)
                    .ToList();

                // NEW: Retrieve Rental Property Postings
                // Business Purpose: Include periodic rental property postings (e.g., rent accruals, security deposits)
                // that affect the party's balance, similar to opening balances.
                // These are posted accounting transactions that should impact the net amount owed.

                var rentalPropertyEntries = await _context.AcctgTransEntries
                    .Where(ate => ate.AcctgTrans.AcctgTransTypeId == "RENTAL_PROPERTY_POSTINGS"
                                  && ate.AcctgTrans.PartyId == request.PartyId
                                  && ate.AcctgTrans.IsPosted == "Y"
                                  && ate.DebitCreditFlag == "D") // ← Only debits
                    .Select(ate => new
                    {
                        ate.AcctgTrans.TransactionDate,
                        ate.GlAccountTypeId,
                        ate.DebitCreditFlag,
                        ate.Amount,
                        CurrencyUomId = !string.IsNullOrEmpty(ate.CurrencyUomId)
                            ? ate.CurrencyUomId
                            : (!string.IsNullOrEmpty(ate.OrigCurrencyUomId)
                                ? ate.OrigCurrencyUomId
                                : currencyUomId)
                    })
                    .ToListAsync(cancellationToken);

                decimal rentalPropertyDebitImpact = 0m;
                var rentalPropertyDtos = new List<RentalPropertyPostingDto>();

                foreach (var entry in rentalPropertyEntries)
                {
                    decimal amount = (decimal)entry.Amount; // already debit → positive impact on receivable

                    // Optional: you may still want to check account type, but since we filtered Debit only
                    // most systems post debit to AR and credit to revenue/income
                    bool isReceivable = IsReceivableAccountType(entry.GlAccountTypeId);

                    // Usually true for debits in this transaction type, but keep logic for safety
                    decimal impact = isReceivable ? amount : -amount;

                    rentalPropertyDebitImpact += Math.Round(impact, 2, MidpointRounding.AwayFromZero);

                    rentalPropertyDtos.Add(new RentalPropertyPostingDto
                    {
                        TransactionDate = entry.TransactionDate,
                        GlAccountTypeId = entry.GlAccountTypeId,
                        Amount = amount,
                        DebitCreditFlag = entry.DebitCreditFlag,
                        CurrencyUomId = entry.CurrencyUomId,
                        ImpactOnBalance = Math.Round(impact, 2, MidpointRounding.AwayFromZero),
                        Description = "Rental Posting (Debit)"
                    });
                }

                rentalPropertyDtos = rentalPropertyDtos
                    .OrderBy(x => x.TransactionDate)
                    .ToList();

                // ───────────────────────────────────────────────────────────────
// 2. Rental Property Accruals for Partners – Credit side only
//    (recognized revenue share belonging to partners)
// ───────────────────────────────────────────────────────────────
                var partnerAccrualEntries = await _context.AcctgTransEntries
                    .Where(ate => ate.AcctgTrans.AcctgTransTypeId == "RENTAL_PROPERTY_ACCRUAL_PARTNERS"
                                  && ate.AcctgTrans.PartyId == request.PartyId
                                  && ate.AcctgTrans.IsPosted == "Y"
                                  && ate.DebitCreditFlag == "C") // ← Only credits
                    .Select(ate => new
                    {
                        ate.AcctgTrans.TransactionDate,
                        ate.GlAccountTypeId,
                        ate.DebitCreditFlag,
                        ate.Amount,
                        CurrencyUomId = !string.IsNullOrEmpty(ate.CurrencyUomId)
                            ? ate.CurrencyUomId
                            : (!string.IsNullOrEmpty(ate.OrigCurrencyUomId)
                                ? ate.OrigCurrencyUomId
                                : currencyUomId)
                    })
                    .ToListAsync(cancellationToken);

                var partnerAccrualDtos = new List<PartnerAccrualPostingDto>(); // ← You'll need to define this DTO
                decimal partnerAccrualCreditImpact = 0m;

                foreach (var entry in partnerAccrualEntries)
                {
                    decimal signedAmount = (decimal)-entry.Amount; // Credit → negative from company perspective

                    bool isReceivable = IsReceivableAccountType(entry.GlAccountTypeId);

                    // Most likely posted to liability/equity/partner account → not receivable
                    // So we usually reverse sign again → becomes positive outflow/share to partners
                    decimal impactOnCustomerBalance = isReceivable ? signedAmount : -signedAmount;

                    partnerAccrualCreditImpact += Math.Round(impactOnCustomerBalance, 2, MidpointRounding.AwayFromZero);

                    partnerAccrualDtos.Add(new PartnerAccrualPostingDto
                    {
                        TransactionDate = entry.TransactionDate,
                        GlAccountTypeId = entry.GlAccountTypeId,
                        Amount = (decimal)entry.Amount,
                        DebitCreditFlag = entry.DebitCreditFlag,
                        CurrencyUomId = entry.CurrencyUomId,
                        ImpactOnBalance = Math.Round(impactOnCustomerBalance, 2, MidpointRounding.AwayFromZero),
                        Description = "Partner Revenue Accrual (Credit)"
                    });
                }

                partnerAccrualDtos = partnerAccrualDtos
                    .OrderBy(x => x.TransactionDate)
                    .ToList();

                // 3. Retrieve Invoices and Applied Payments
                // Benefit: Filters valid invoices and payments, ensuring only PMNT_RECEIVED payments are applied, preventing invalid applications (e.g., PMNT_NOT_PAID)
                // Wisdom: Aligns with OFBiz's expectation of received payments for applications, ensuring accurate financial reporting
                var invoicesApplPaymentsQuery = from inv in _context.Invoices
                    join pap in _context.PaymentApplications on inv.InvoiceId equals pap.InvoiceId into papGroup
                    from pap in papGroup.DefaultIfEmpty()
                    join pmt in _context.Payments on pap.PaymentId equals pmt.PaymentId into pmtGroup
                    from pmt in pmtGroup.DefaultIfEmpty()
                    where ((inv.PartyId == request.PartyId && inv.PartyIdFrom == organizationPartyId) ||
                           (inv.PartyId == organizationPartyId && inv.PartyIdFrom == request.PartyId))
                          && inv.StatusId != "INVOICE_IN_PROCESS"
                          && inv.StatusId != "INVOICE_CANCELLED"
                          && inv.StatusId != "INVOICE_WRITEOFF"
                    select new
                    {
                        inv.InvoiceId,
                        inv.InvoiceTypeId,
                        InvoiceDate = inv.InvoiceDate, // Normalize date
                        PaymentId = pmt != null ? pmt.PaymentId : null,
                        PaymentEffectiveDate = pmt != null ? pmt.EffectiveDate : (DateTime?)null, // Normalize date
                        PaymentAppliedAmount = pap != null ? pap.AmountApplied : 0,
                        PaymentAmount = pmt != null ? pmt.Amount : 0,
                        CurrencyUomId = inv.CurrencyUomId ?? (pmt != null ? pmt.ActualCurrencyUomId : currencyUomId)
                    };

                var invoicesApplPayments = await invoicesApplPaymentsQuery
                    .ToListAsync(cancellationToken);

                // Business Purpose: Transform invoice and payment data into DTOs, calculating totals and applied amounts.
                // This provides a structured format for the financial history, including applied and unapplied amounts for reporting.

                var invoicesApplPaymentsDtos = new List<InvoiceApplPaymentDto>();
                foreach (var item in invoicesApplPayments)
                {
                    decimal total = await _invoiceUtilityService.GetInvoiceTotal(item.InvoiceId, actualCurrency);
                    decimal amountApplied = (decimal)(item.PaymentAppliedAmount > 0
                        ? item.PaymentAppliedAmount
                        : await _invoiceUtilityService.GetInvoiceApplied(item.InvoiceId, DateTime.UtcNow,
                            actualCurrency));
                    decimal amountToApply = await _invoiceUtilityService.GetInvoiceNotApplied(item.InvoiceId);

                    invoicesApplPaymentsDtos.Add(new InvoiceApplPaymentDto
                    {
                        InvoiceId = item.InvoiceId,
                        InvoiceTypeId = item.InvoiceTypeId,
                        InvoiceDate = item.InvoiceDate,
                        Total = total,
                        AmountApplied = amountApplied,
                        AmountToApply = amountToApply,
                        PaymentId = item.PaymentId,
                        PaymentEffectiveDate = item.PaymentEffectiveDate,
                        PaymentAmount = item.PaymentAmount,
                        CurrencyUomId = item.CurrencyUomId
                    });
                }

                invoicesApplPaymentsDtos = invoicesApplPaymentsDtos
                    .OrderBy(x => x.InvoiceDate)
                    .ToList();

                // 4. Retrieve Unapplied Invoices
                // Business Purpose: Retrieve unapplied invoices to identify outstanding amounts owed by or to the party.
                // Filtering ensures only valid, non-cancelled invoices are included, with date constraints for relevance.
                var unappliedInvoicesQuery = from inv in _context.Invoices
                    join it in _context.InvoiceTypes on inv.InvoiceTypeId equals it.InvoiceTypeId
                    where ((inv.PartyId == request.PartyId && inv.PartyIdFrom == organizationPartyId) ||
                           (inv.PartyId == organizationPartyId && inv.PartyIdFrom == request.PartyId))
                          && inv.StatusId != "INVOICE_IN_PROCESS"
                          && inv.StatusId != "INVOICE_CANCELLED"
                          && inv.StatusId != "INVOICE_WRITEOFF"
                    select new
                    {
                        inv.InvoiceId,
                        TypeDescription = it.Description,
                        inv.InvoiceDate,
                        inv.InvoiceTypeId,
                        ParentTypeId = it.ParentTypeId,
                        CurrencyUomId = actualCurrency ? inv.CurrencyUomId : request.DefaultCurrencyUomId
                    };

                var unappliedInvoices = await unappliedInvoicesQuery
                    .ToListAsync(cancellationToken);

                var unappliedInvoicesDtos = new List<UnappliedInvoiceDto>();
                foreach (var item in unappliedInvoices)
                {
                    decimal amount =
                        Math.Round(await _invoiceUtilityService.GetInvoiceTotal(item.InvoiceId, actualCurrency), 2,
                            MidpointRounding.AwayFromZero);
                    decimal unappliedAmount =
                        Math.Round(await _invoiceUtilityService.GetInvoiceNotApplied(item.InvoiceId), 2,
                            MidpointRounding.AwayFromZero);

                    if (unappliedAmount > 0)
                    {
                        unappliedInvoicesDtos.Add(new UnappliedInvoiceDto
                        {
                            InvoiceId = item.InvoiceId,
                            TypeDescription = item.TypeDescription,
                            InvoiceDate = item.InvoiceDate,
                            Amount = amount,
                            UnappliedAmount = unappliedAmount,
                            CurrencyUomId = item.CurrencyUomId ?? request.DefaultCurrencyUomId,
                            InvoiceTypeId = item.InvoiceTypeId,
                            InvoiceParentTypeId = item.ParentTypeId
                        });
                    }
                }

                unappliedInvoicesDtos = unappliedInvoicesDtos
                    .OrderBy(x => x.InvoiceDate)
                    .ToList();

                // 5. Retrieve Unapplied Payments
                // Business Purpose: Retrieve unapplied payments to identify payments not yet allocated to invoices.
                // This helps in understanding available funds for future invoice applications.

                var unappliedPaymentsQuery = from pmt in _context.Payments
                    join pt in _context.PaymentTypes on pmt.PaymentTypeId equals pt.PaymentTypeId
                    where ((pmt.PartyIdTo == request.PartyId && pmt.PartyIdFrom == organizationPartyId) ||
                           (pmt.PartyIdTo == organizationPartyId && pmt.PartyIdFrom == request.PartyId)) &&
                          pmt.StatusId != "PMNT_NOT_PAID" &&
                          pmt.StatusId != "PMNT_CANCELLED"
                    select new
                    {
                        pmt.PaymentId,
                        pmt.EffectiveDate,
                        pmt.PaymentTypeId,
                        PaymentTypeDescription = pt.Description,
                        pmt.ActualCurrencyAmount,
                        pmt.ActualCurrencyUomId,
                        pmt.Amount,
                        CurrencyUomId = pmt.CurrencyUomId,
                        pmt.PaymentType.ParentTypeId
                    };

                var unappliedPayments = await unappliedPaymentsQuery
                    .ToListAsync(cancellationToken);

                var unappliedPaymentsDtos = new List<UnappliedPaymentDto>();
                foreach (var item in unappliedPayments)
                {
                    decimal unappliedAmount =
                        Math.Round(await _paymentWorkerService.GetPaymentNotApplied(item.PaymentId, actualCurrency), 2,
                            MidpointRounding.AwayFromZero);
                    if (unappliedAmount > 0)
                    {
                        decimal amount = actualCurrency && item.ActualCurrencyAmount.HasValue &&
                                         !string.IsNullOrEmpty(item.ActualCurrencyUomId)
                            ? item.ActualCurrencyAmount.Value
                            : item.Amount;
                        string paymentCurrencyUomId = actualCurrency && !string.IsNullOrEmpty(item.ActualCurrencyUomId)
                            ? item.ActualCurrencyUomId
                            : item.CurrencyUomId ?? request.DefaultCurrencyUomId;

                        unappliedPaymentsDtos.Add(new UnappliedPaymentDto
                        {
                            PaymentId = item.PaymentId,
                            EffectiveDate = item.EffectiveDate,
                            PaymentTypeId = item.PaymentTypeId,
                            PaymentTypeDescription = item.PaymentTypeDescription,
                            Amount = amount,
                            UnappliedAmount = unappliedAmount,
                            CurrencyUomId = paymentCurrencyUomId,
                            PaymentParentTypeId = item.ParentTypeId
                        });
                    }
                }

                unappliedPaymentsDtos = unappliedPaymentsDtos
                    .OrderBy(x => x.EffectiveDate)
                    .ToList();

                // 6. Retrieve Billing Accounts
                // Business Purpose: Retrieve billing accounts associated with the party to include their balances and limits.
                // Filtering by date and role ensures only active, relevant billing accounts are considered.
                string billingCurrencyUomId = currencyUomId;
                var billingAccountRoles = await _context.BillingAccountRoles
                    .Where(bar =>
                        bar.PartyId == request.PartyId &&
                        bar.RoleTypeId == "BILL_TO_CUSTOMER" &&
                        bar.FromDate <= DateTime.UtcNow &&
                        (bar.ThruDate == null || bar.ThruDate >= DateTime.UtcNow))
                    .Join(_context.BillingAccounts,
                        bar => bar.BillingAccountId,
                        ba => ba.BillingAccountId,
                        (bar, ba) => new { bar, ba })
                    .ToListAsync(cancellationToken);

                if (billingAccountRoles.Any())
                {
                    billingCurrencyUomId = billingAccountRoles.First().ba.AccountCurrencyUomId ?? currencyUomId;
                }

                var billingAccounts =
                    await _billingAccountService.MakePartyBillingAccountList(request.PartyId, billingCurrencyUomId);

                // Business Purpose: Calculate billing account balances by summing payment applications and invoice totals.
                // This provides an accurate account balance for financial reporting.
                var billingAccountsDtos = new List<BillingAccountDto>();
                foreach (var ba in billingAccounts)
                {
                    // Sum PaymentApplication amounts
                    decimal balance = await _context.PaymentApplications
                        .Where(pa => pa.BillingAccountId == ba.BillingAccountId)
                        .SumAsync(pa => pa.AmountApplied, cancellationToken) ?? 0;

                    // Get invoice IDs and calculate total asynchronously
                    var invoiceIds = await _context.Invoices
                        .Where(inv => inv.BillingAccountId == ba.BillingAccountId)
                        .Select(inv => inv.InvoiceId)
                        .ToListAsync(cancellationToken);

                    decimal invoiceTotal = 0;
                    if (invoiceIds.Any())
                    {
                        var invoiceTotals = await Task.WhenAll(
                            invoiceIds.Select(id => _invoiceUtilityService.GetInvoiceTotal(id, actualCurrency))
                        );
                        invoiceTotal = invoiceTotals.Sum();
                    }

                    balance -= invoiceTotal;

                    billingAccountsDtos.Add(new BillingAccountDto
                    {
                        BillingAccountId = ba.BillingAccountId,
                        AccountLimit = ba.AccountLimit,
                        AccountBalance = ba.AccountBalance,
                        Description = ba.Description
                    });
                }

                // 7. Retrieve Returns
                // Business Purpose: Retrieve return records to include in the financial history.
                // This ensures all relevant financial transactions, including returns, are reported.

                var returns = await _context.ReturnHeaders
                    .Where(rh => rh.FromPartyId == request.PartyId)
                    .Join(_context.StatusItems,
                        rh => rh.StatusId,
                        si => si.StatusId,
                        (rh, si) => new ReturnDto
                        {
                            ReturnId = rh.ReturnId,
                            StatusDescription = si.Description,
                            FromPartyId = rh.FromPartyId,
                            ToPartyId = rh.ToPartyId
                        })
                    .OrderBy(x => x.ReturnId)
                    .ToListAsync(cancellationToken);

                // 8. Calculate Financial Summary
                // Business Purpose: Calculate a financial summary to provide an overview of sales and purchase invoices, payments, and outstanding amounts.
                // This aggregates key financial metrics for quick reference.

                decimal totalSalesInvoice = 0, totalPurchaseInvoice = 0;
                decimal totalSalesNotApplied = 0, totalPurchaseNotApplied = 0;

                var invoiceSummaryQuery = from inv in _context.Invoices
                    where ((inv.PartyId == request.PartyId && inv.PartyIdFrom == organizationPartyId) ||
                           (inv.PartyId == organizationPartyId && inv.PartyIdFrom == request.PartyId))
                          && inv.StatusId != "INVOICE_IN_PROCESS"
                          && inv.StatusId != "INVOICE_CANCELLED"
                          && inv.StatusId != "INVOICE_WRITEOFF"
                    select new { inv.InvoiceId, inv.InvoiceTypeId };

                var invoicesForSummary = await invoiceSummaryQuery.ToListAsync(cancellationToken);

                foreach (var inv in invoicesForSummary)
                {
                    decimal total = await _invoiceUtilityService.GetInvoiceTotal(inv.InvoiceId, actualCurrency);
                    decimal applied =
                        await _invoiceUtilityService.GetInvoiceApplied(inv.InvoiceId, DateTime.UtcNow, actualCurrency);
                    decimal notApplied = total - applied;

                    if (inv.InvoiceTypeId == "SALES_INVOICE")
                    {
                        totalSalesInvoice += Math.Round(total, 2, MidpointRounding.AwayFromZero);
                        totalSalesNotApplied += Math.Round(notApplied, 2, MidpointRounding.AwayFromZero);
                    }
                    else if (inv.InvoiceTypeId == "PURCHASE_INVOICE")
                    {
                        totalPurchaseInvoice += Math.Round(total, 2, MidpointRounding.AwayFromZero);
                        totalPurchaseNotApplied += Math.Round(notApplied, 2, MidpointRounding.AwayFromZero);
                    }
                }

                // Business Purpose: Summarize payment data to distinguish between incoming and outgoing payments, both applied and unapplied.
                // This provides a clear picture of cash flow related to the party.

                decimal totalPayInApplied = 0,
                    totalPayInNotApplied = 0,
                    totalPayOutApplied = 0,
                    totalPayOutNotApplied = 0;
                var paymentSummaryQuery = from pmt in _context.Payments
                    join pt in _context.PaymentTypes on pmt.PaymentTypeId equals pt.PaymentTypeId
                    where ((pmt.PartyIdTo == request.PartyId && pmt.PartyIdFrom == organizationPartyId) ||
                           (pmt.PartyIdTo == organizationPartyId && pmt.PartyIdFrom == request.PartyId)) &&
                          pmt.StatusId != "PMNT_NOT_PAID" &&
                          pmt.StatusId != "PMNT_CANCELLED"
                    select new { pmt.PaymentId, pt.ParentTypeId };

                var paymentsForSummary = await paymentSummaryQuery.ToListAsync(cancellationToken);

                foreach (var pmt in paymentsForSummary)
                {
                    bool isDisbursement = pmt.ParentTypeId == "DISBURSEMENT" || pmt.ParentTypeId == "TAX_PAYMENT";
                    bool isReceipt = pmt.ParentTypeId == "RECEIPT";

                    decimal applied = await _paymentWorkerService.GetPaymentApplied(pmt.PaymentId, actualCurrency);
                    decimal notApplied =
                        await _paymentWorkerService.GetPaymentNotApplied(pmt.PaymentId, actualCurrency);

                    if (isDisbursement)
                    {
                        totalPayOutApplied += Math.Round(applied, 2, MidpointRounding.AwayFromZero);
                        totalPayOutNotApplied += Math.Round(notApplied, 2, MidpointRounding.AwayFromZero);
                    }
                    else if (isReceipt)
                    {
                        totalPayInApplied += Math.Round(applied, 2, MidpointRounding.AwayFromZero);
                        totalPayInNotApplied += Math.Round(notApplied, 2, MidpointRounding.AwayFromZero);
                    }
                }

                // Business Purpose: Create a financial summary DTO to encapsulate key financial metrics, including net amounts to be paid or received.
                // This simplifies the interpretation of the party's financial position.

                var financialSummary = new FinancialSummaryDto
                {
                    TotalSalesInvoice = totalSalesInvoice,
                    TotalPurchaseInvoice = totalPurchaseInvoice,
                    TotalPaymentsIn = totalPayInApplied + totalPayInNotApplied,
                    TotalPaymentsOut = totalPayOutApplied + totalPayOutNotApplied,
                    TotalInvoiceNotApplied = totalSalesNotApplied - totalPurchaseNotApplied,
                    TotalPaymentNotApplied = totalPayInNotApplied - totalPayOutNotApplied
                };

                decimal transferAmount = financialSummary.TotalSalesInvoice
                                         - financialSummary.TotalPurchaseInvoice
                                         - financialSummary.TotalPaymentsIn
                                         + financialSummary.TotalPaymentsOut
                                         + rentalPropertyDebitImpact 
                                         - partnerAccrualCreditImpact;

                if (transferAmount > 0)
                {
                    financialSummary.TotalToBeReceived = Math.Round(transferAmount, 2, MidpointRounding.AwayFromZero);
                    financialSummary.TotalToBePaid = 0m;
                }
                else if (transferAmount < 0)
                {
                    financialSummary.TotalToBePaid = Math.Round(-transferAmount, 2, MidpointRounding.AwayFromZero);
                    financialSummary.TotalToBeReceived = 0m;
                }
                else
                {
                    financialSummary.TotalToBeReceived = 0m;
                    financialSummary.TotalToBePaid = 0m;
                }

                // 9. Return result
                // Business Purpose: Compile all retrieved data into a single response object for the client.
                // This provides a comprehensive view of the party's financial history, including invoices, payments, billing accounts, returns, and summary.

                return Result<PartyFinancialHistoryDetails>.Success(new PartyFinancialHistoryDetails
                {
                    PartyId = request.PartyId,
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
            }
            catch (Exception ex)
            {
                return Result<PartyFinancialHistoryDetails>.Failure(
                    $"Error retrieving party financial history: {ex.Message}");
            }
        }
    }
}

public class OpeningBalanceDto
{
    public DateTime? TransactionDate { get; set; }
    public string GlAccountTypeId { get; set; }
    public decimal Amount { get; set; }
    public string DebitCreditFlag { get; set; } // "D" or "C"
    public string CurrencyUomId { get; set; }
    public decimal ImpactOnBalance { get; set; } // Positive = customer owes us
    public string Description { get; set; }
}

public class RentalPropertyPostingDto
{
    public DateTime? TransactionDate { get; set; }
    public string GlAccountTypeId { get; set; }
    public decimal Amount { get; set; }
    public string DebitCreditFlag { get; set; } // "D" or "C"
    public string CurrencyUomId { get; set; }
    public decimal ImpactOnBalance { get; set; } // Positive = customer owes us more
    public string Description { get; set; } = "Rental Property Posting";
}

public class PartnerAccrualPostingDto
{
    public DateTime? TransactionDate { get; set; }
    public string? GlAccountTypeId { get; set; }
    public decimal Amount { get; set; }
    public string? DebitCreditFlag { get; set; }        // "C" in this case
    public string? CurrencyUomId { get; set; }
    public decimal ImpactOnBalance { get; set; }        // Usually negative (reduces net receivable)
    public string Description { get; set; } = "استحقاق إيراد العقار للشركاء";
}