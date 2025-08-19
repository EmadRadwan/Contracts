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

                // 3. Retrieve Invoices and Applied Payments
                // Benefit: Filters valid invoices and payments, ensuring only PMNT_RECEIVED payments are applied, preventing invalid applications (e.g., PMNT_NOT_PAID)
                // Wisdom: Aligns with OFBiz's expectation of received payments for applications, ensuring accurate financial reporting
                var invoicesApplPaymentsQuery = from inv in _context.Invoices
                    join pap in _context.PaymentApplications on inv.InvoiceId equals pap.InvoiceId into papGroup
                    from pap in papGroup.DefaultIfEmpty()
                    join pmt in _context.Payments on pap.PaymentId equals pmt.PaymentId into pmtGroup
                    from pmt in pmtGroup.DefaultIfEmpty()
                    where ((inv.PartyId == request.PartyId && inv.PartyIdFrom == organizationPartyId) ||
                           (inv.PartyId == organizationPartyId && inv.PartyIdFrom == request.PartyId)) &&
                          inv.StatusId != "INVOICE_IN_PROCESS" &&
                          inv.StatusId != "INVOICE_CANCELLED" &&
                          inv.StatusId != "INVOICE_WRITEOFF" &&
                          inv.InvoiceDate <= DateTime.UtcNow.Date && // Normalize date to avoid timestamp issues
                          (pmt == null || pmt.StatusId == "PMNT_RECEIVED") // Validate payment status
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
                           (inv.PartyId == organizationPartyId && inv.PartyIdFrom == request.PartyId)) &&
                          inv.StatusId != "INVOICE_IN_PROCESS" &&
                          inv.StatusId != "INVOICE_CANCELLED" &&
                          inv.StatusId != "INVOICE_WRITEOFF" &&
                          inv.InvoiceDate <= DateTime.UtcNow
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
                          pmt.StatusId != "PMNT_NOTPAID" &&
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
                decimal totalInvSaApplied = 0,
                    totalInvSaNotApplied = 0,
                    totalInvPuApplied = 0,
                    totalInvPuNotApplied = 0;
                var invoiceSummaryQuery = from inv in _context.Invoices
                    join it in _context.InvoiceTypes on inv.InvoiceTypeId equals it.InvoiceTypeId
                    where ((inv.PartyId == request.PartyId && inv.PartyIdFrom == organizationPartyId) ||
                           (inv.PartyId == organizationPartyId && inv.PartyIdFrom == request.PartyId)) &&
                          inv.StatusId != "INVOICE_IN_PROCESS" &&
                          inv.StatusId != "INVOICE_CANCELLED" &&
                          inv.StatusId != "INVOICE_WRITEOFF" &&
                          inv.InvoiceDate <= DateTime.UtcNow &&
                          (inv.InvoiceDate == null || inv.InvoiceDate >= DateTime.UtcNow)
                    select new { inv.InvoiceId, inv.InvoiceTypeId, it.ParentTypeId };

                var invoicesForSummary = await invoiceSummaryQuery.ToListAsync(cancellationToken);

                foreach (var inv in invoicesForSummary)
                {
                    bool isSalesInvoice = inv.ParentTypeId == "SALES_INVOICE";
                    bool isPurchaseInvoice = inv.ParentTypeId == "PURCHASE_INVOICE";

                    decimal applied =
                        await _invoiceUtilityService.GetInvoiceApplied(inv.InvoiceId, DateTime.UtcNow, actualCurrency);
                    decimal notApplied = await _invoiceUtilityService.GetInvoiceNotApplied(inv.InvoiceId);

                    if (isSalesInvoice)
                    {
                        totalInvSaApplied += Math.Round(applied, 2, MidpointRounding.AwayFromZero);
                        totalInvSaNotApplied += Math.Round(notApplied, 2, MidpointRounding.AwayFromZero);
                    }
                    else if (isPurchaseInvoice)
                    {
                        totalInvPuApplied += Math.Round(applied, 2, MidpointRounding.AwayFromZero);
                        totalInvPuNotApplied += Math.Round(notApplied, 2, MidpointRounding.AwayFromZero);
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
                          pmt.StatusId != "PMNT_NOTPAID" &&
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
                    TotalSalesInvoice = totalInvSaApplied + totalInvSaNotApplied,
                    TotalPurchaseInvoice = totalInvPuApplied + totalInvPuNotApplied,
                    TotalPaymentsIn = totalPayInApplied + totalPayInNotApplied,
                    TotalPaymentsOut = totalPayOutApplied + totalPayOutNotApplied,
                    TotalInvoiceNotApplied = totalInvSaNotApplied - totalInvPuNotApplied,
                    TotalPaymentNotApplied = totalPayInNotApplied - totalPayOutNotApplied
                };

                decimal transferAmount = financialSummary.TotalSalesInvoice - financialSummary.TotalPurchaseInvoice
                                                                            - financialSummary.TotalPaymentsIn -
                                                                            financialSummary.TotalPaymentsOut;

                if (transferAmount < 0)
                {
                    financialSummary.TotalToBeReceived = -transferAmount;
                }
                else
                {
                    financialSummary.TotalToBePaid = transferAmount;
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