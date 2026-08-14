using Application.Accounting.Services.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.Services;

public class GetGlAccountTransactionDetails
{
    public class Query : IRequest<Result<GlAccountTransactionDetails>>
    {
        public string CustomTimePeriodId { get; set; }
        public string OrganizationPartyId { get; set; }
        public string GlAccountId { get; set; }
        public bool IncludePrePeriodTransactions { get; set; } = true;

        // When false (default), a SHIPMENT_RECEIPT that was later found to duplicate a
        // PURCHASE_INVOICE's ACCOUNTS_PAYABLE credit — and its matching
        // RemediateSupplyCertificateDuplicateAp correction — are hidden as a pair, since the
        // correction was sized to exactly cancel the receipt and the two only exist because of that
        // historical fix. Set true to see the full, uncollapsed posting history instead.
        public bool ShowCorrectedDuplicatePairs { get; set; } = false;
    }

    public class Handler : IRequestHandler<Query, Result<GlAccountTransactionDetails>>
    {
        private readonly DataContext _context;
        private readonly IAcctgMiscService _acctgMiscService;

        public Handler(DataContext context, IAcctgMiscService acctgMiscService)
        {
            _context = context;
            _acctgMiscService = acctgMiscService;
        }

        public async Task<Result<GlAccountTransactionDetails>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            try
            {
                // 1. Retrieve CustomTimePeriod
                var customTimePeriod = await _context.CustomTimePeriods
                    .FindAsync(request.CustomTimePeriodId);

                if (customTimePeriod == null)
                {
                    return Result<GlAccountTransactionDetails>.Failure("CustomTimePeriod not found.");
                }

                // 2. Retrieve GL Account
                var glAccount = await _context.GlAccounts
                    .FindAsync(request.GlAccountId);

                if (glAccount == null)
                {
                    return Result<GlAccountTransactionDetails>.Failure("GlAccount not found.");
                }

                var periodStart = customTimePeriod.FromDate.Value.Date; // e.g. 2026-01-01 00:00:00
                var periodEnd = customTimePeriod.ThruDate.Value.Date; // inclusive end

                // 3. Build query for transactions
                var query = from ate in _context.AcctgTransEntries
                    join act in _context.AcctgTrans on ate.AcctgTransId equals act.AcctgTransId
                    join att in _context.AcctgTransTypes on act.AcctgTransTypeId equals att.AcctgTransTypeId into
                        transTypes
                    from att in transTypes.DefaultIfEmpty()
                    join p in _context.Parties on act.PartyId equals p.PartyId into parties
                    from p in parties.DefaultIfEmpty()
                    join prod in _context.Products on ate.ProductId equals prod.ProductId into products
                    from prod in products.DefaultIfEmpty()
                    join we in _context.WorkEfforts on act.WorkEffortId equals we.WorkEffortId into workEfforts
                    from we in workEfforts.DefaultIfEmpty()
                    join project in _context.WorkEfforts
                        on we.ProjectId equals project.WorkEffortId
                        into projects
                    from project in projects.DefaultIfEmpty()
                    join pyt in _context.Payments on act.PaymentId equals pyt.PaymentId into payments
                    from pyt in payments.DefaultIfEmpty()
                    join cc in _context.CostCenters on pyt.CostCenterId equals cc.CostCenterId into costCenters
                    from cc in costCenters.DefaultIfEmpty()
                    where ate.OrganizationPartyId == request.OrganizationPartyId
                          && ate.GlAccountId == request.GlAccountId
                          && act.IsPosted == "Y"
                          && act.GlFiscalTypeId == "ACTUAL" && act.AcctgTransTypeId != "PAYMENT_APPL"
                    select new TransactionEntryDto
                    {
                        AcctgTransId = ate.AcctgTransId,
                        TransactionDate = (DateTime)act.TransactionDate,
                        AcctgTransTypeId = act.AcctgTransTypeId ?? "Unknown",
                        AcctgTransTypeDescription = att != null ? att.Description : (act.AcctgTransTypeId ?? "Unknown"),
                        InvoiceId = act.InvoiceId,
                        PaymentId = we != null && we.WorkEffortTypeId == "PAYMENT_CERTIFICATE"
                            ? we.WorkEffortId
                            : act.PaymentId,
                        WorkEffortId = act.WorkEffortId,
                        ShipmentId = act.ShipmentId,
                        PartyId = act.PartyId,
                        PartyName = p != null ? p.Description : null,
                        ProductId = ate.ProductId,
                        ProductName = prod != null ? prod.ProductName : null,
                        IsPosted = act.IsPosted,
                        PostedDate = act.PostedDate,
                        DebitCreditFlag = ate.DebitCreditFlag,
                        Amount = (decimal)ate.Amount,
                        Description = ate.Description,
                        CurrencyUomId = ate.CurrencyUomId,
                        CertificateNumber = we != null ? we.CertificateNumber : null,
                        ProjectName = we != null
                            ? (we.WorkEffortTypeId == "PROJECT" 
                                ? (we.ProjectName ?? we.Description ?? we.WorkEffortName)
                                : (project != null ? (project.ProjectName ?? project.Description ?? project.WorkEffortName) : null))
                            : null,
                        CostCenterDescription = cc != null ? cc.Description : null,
                        PaymentRefNum = we != null && we.WorkEffortTypeId == "PAYMENT_CERTIFICATE"
                            ? we.Notes
                            : (pyt != null ? pyt.PaymentRefNum : null),
                    };

                // 4. Filter transactions for display (respect IncludePrePeriodTransactions)
                IQueryable<TransactionEntryDto> transactionsQuery;

                if (request.IncludePrePeriodTransactions)
                {
                    // Show everything up to (but not including) ThruDate + 1
                    transactionsQuery = query.Where(x => x.TransactionDate <= periodEnd);
                }
                else
                {
                    // Only current period
                    transactionsQuery = query.Where(x =>
                        x.TransactionDate >= periodStart &&
                        x.TransactionDate <= periodEnd);
                }

                var transactions = await transactionsQuery
                    .OrderBy(x => x.TransactionDate)
                    .ThenBy(x => x.AcctgTransId)
                    .ToListAsync(cancellationToken);

                // 4b. Hide corrected duplicate pairs (default) — a SHIPMENT_RECEIPT that
                // RemediateSupplyCertificateDuplicateAp later found duplicating a PURCHASE_INVOICE's
                // ACCOUNTS_PAYABLE credit, plus the INTERNAL_ACCTG_TRANS correction posted for it.
                //
                // A pair may only be collapsed on an account where it is SELF-CANCELLING — the hidden
                // rows' debits must equal their credits on THIS account. That holds on the AP accounts
                // the remediation was written for (receipt credits AP, correction debits it back), but
                // NOT elsewhere: the receipt also debits INVENTORY (140000) while the correction
                // credits UNINVOICED_SHIP_RCPT (214000), so on each of those accounts only one leg of
                // the pair is present. Hiding it unopposed used to drop 3,458,216.00 off both accounts'
                // EndingBalance — reporting GL 140000 at -3,458,216.00 when it is actually 0.00.
                //
                // So the pair is evaluated per work effort, and only dropped when it nets to zero here.
                decimal hiddenDebitTotal = 0m;
                decimal hiddenCreditTotal = 0m;

                if (!request.ShowCorrectedDuplicatePairs)
                {
                    var corrections = await _context.AcctgTrans
                        .Where(t => t.AcctgTransTypeId == "INTERNAL_ACCTG_TRANS" &&
                                    t.Description == "DUPLICATE_AP_CORRECTION_SUPPLY_CERTIFICATE" &&
                                    t.IsPosted == "Y")
                        .Select(t => new { t.AcctgTransId, t.WorkEffortId })
                        .ToListAsync(cancellationToken);

                    var correctionAcctgTransIds = corrections.Select(c => c.AcctgTransId).ToHashSet();
                    var correctedWorkEffortIds = corrections
                        .Where(c => !string.IsNullOrEmpty(c.WorkEffortId))
                        .Select(c => c.WorkEffortId!)
                        .ToHashSet();

                    var candidates = transactions.Where(t =>
                        !string.IsNullOrEmpty(t.WorkEffortId) &&
                        correctedWorkEffortIds.Contains(t.WorkEffortId) &&
                        (correctionAcctgTransIds.Contains(t.AcctgTransId) ||
                         t.AcctgTransTypeId == "SHIPMENT_RECEIPT"));

                    var hidden = candidates
                        .GroupBy(t => t.WorkEffortId!)
                        .Where(g => g.Where(t => t.DebitCreditFlag == "D").Sum(t => t.Amount) ==
                                    g.Where(t => t.DebitCreditFlag == "C").Sum(t => t.Amount))
                        .SelectMany(g => g)
                        .ToList();

                    hiddenDebitTotal = hidden.Where(t => t.DebitCreditFlag == "D").Sum(t => t.Amount);
                    hiddenCreditTotal = hidden.Where(t => t.DebitCreditFlag == "C").Sum(t => t.Amount);

                    transactions = transactions.Except(hidden).ToList();
                }

                // 5. Calculate aggregates over exactly the window the rows above cover.
                //
                // The seed and the displayed rows must PARTITION the timeline — no overlap, no gap —
                // and the totals must span exactly the same window as the rows. Three rules follow:
                //
                //   windowStart      = where the row list begins (moves back when the user asks to
                //                      see pre-period rows)
                //   BroughtForward   = signed sum of ALL activity strictly BEFORE windowStart,
                //                      whatever its transaction type
                //   Posted debits/credits = activity within [windowStart, periodEnd]
                //
                // BroughtForward used to be defined by transaction TYPE (only OPENING_BALANCE) rather
                // than by DATE, which broke in two ways at once on any account holding a reset entry:
                // the OPENING_BALANCE row is itself dated before periodStart, so it seeded the running
                // balance AND appeared in the row list — counted twice — while genuine pre-period
                // movement was shown as rows but excluded from the totals. On GL 110100 that put
                // 22,955,032.00 between the header's EndingBalance and the last running-balance row.
                //
                // Defining it by date also retires the old hasOpeningBalanceEntry heuristic: an account
                // with no reset entry simply carries the sum of its real prior movement, so it no longer
                // needs the totals silently widened to lifetime-to-date to come out right.
                //
                // PAYMENT_APPL is excluded here exactly as it is from the row query above: these must
                // always filter on the same set, or the totals stop agreeing with the rows the user can
                // see and add up. Leaving it out overstated both totals on GL 210041 by 50,000 with no
                // visible rows to explain the gap.
                var windowStart = request.IncludePrePeriodTransactions ? (DateTime?)null : periodStart;

                var broughtForwardDebits = await (
                    from ate in _context.AcctgTransEntries
                    join act in _context.AcctgTrans on ate.AcctgTransId equals act.AcctgTransId
                    where ate.OrganizationPartyId == request.OrganizationPartyId
                          && ate.GlAccountId == request.GlAccountId
                          && act.IsPosted == "Y"
                          && ate.DebitCreditFlag == "D"
                          && act.GlFiscalTypeId == "ACTUAL"
                          && act.AcctgTransTypeId != "PAYMENT_APPL"
                          && windowStart != null && act.TransactionDate < windowStart
                    select ate.Amount
                ).SumAsync(cancellationToken);

                var broughtForwardCredits = await (
                    from ate in _context.AcctgTransEntries
                    join act in _context.AcctgTrans on ate.AcctgTransId equals act.AcctgTransId
                    where ate.OrganizationPartyId == request.OrganizationPartyId
                          && ate.GlAccountId == request.GlAccountId
                          && act.IsPosted == "Y"
                          && ate.DebitCreditFlag == "C"
                          && act.GlFiscalTypeId == "ACTUAL"
                          && act.AcctgTransTypeId != "PAYMENT_APPL"
                          && windowStart != null && act.TransactionDate < windowStart
                    select ate.Amount
                ).SumAsync(cancellationToken);

                var periodDebits = await (
                    from ate in _context.AcctgTransEntries
                    join act in _context.AcctgTrans on ate.AcctgTransId equals act.AcctgTransId
                    where ate.OrganizationPartyId == request.OrganizationPartyId
                          && ate.GlAccountId == request.GlAccountId
                          && act.IsPosted == "Y"
                          && ate.DebitCreditFlag == "D"
                          && act.GlFiscalTypeId == "ACTUAL"
                          && act.AcctgTransTypeId != "PAYMENT_APPL"
                          && (windowStart == null || act.TransactionDate >= windowStart)
                          && act.TransactionDate <= periodEnd
                    select ate.Amount
                ).SumAsync(cancellationToken);

                var periodCredits = await (
                    from ate in _context.AcctgTransEntries
                    join act in _context.AcctgTrans on ate.AcctgTransId equals act.AcctgTransId
                    where ate.OrganizationPartyId == request.OrganizationPartyId
                          && ate.GlAccountId == request.GlAccountId
                          && act.IsPosted == "Y"
                          && ate.DebitCreditFlag == "C"
                          && act.GlFiscalTypeId == "ACTUAL"
                          && act.AcctgTransTypeId != "PAYMENT_APPL"
                          && (windowStart == null || act.TransactionDate >= windowStart)
                          && act.TransactionDate <= periodEnd
                    select ate.Amount
                ).SumAsync(cancellationToken);

                // 6. Determine account side
                bool isDebit = await _acctgMiscService.IsDebitAccount(request.GlAccountId);

                // 7. Calculate balances.
                //
                // Every figure this report hands back is rounded once, here, to the same DisplayScale
                // with the same mode. Only RunningBalance used to be rounded, so an amount carrying
                // more than two decimals — GL 124426 held 32,799.999 from a 4-decimal landed-cost unit
                // price — left the last row reading 3,366,358.06 against an EndingBalance of
                // 3,366,358.059. The ledger's own scale is 4 (GetGlArithmeticSettingsInline), which is
                // posting precision, not display precision; this report presents money at 2.
                //
                // Amounts are rounded for display too — a row reading 32,799.999 beside a balance
                // rounded to 32,800.00 is what made the original sheet look wrong — and the running
                // balance accumulates from those same rounded amounts.
                //
                // Note this leaves sum(rounded amounts) able to differ from round(sum(amounts)) by a
                // cent or two if many rows carry sub-cent tails, since PostedDebits/PostedCredits are
                // still summed in SQL over the un-rounded values. Not reachable on current data (two
                // entries DB-wide exceed two decimals, on one transaction). Deriving the totals from
                // the rows instead would close it for good — the aggregate window and the row set are
                // now identical — but that is a restructure, not a rounding fix.
                const int DisplayScale = 2;
                const MidpointRounding DisplayRounding = MidpointRounding.AwayFromZero;

                decimal openingBalance = Math.Round((decimal)(isDebit
                    ? broughtForwardDebits - broughtForwardCredits
                    : broughtForwardCredits - broughtForwardDebits), DisplayScale, DisplayRounding);

                foreach (var t in transactions)
                    t.Amount = Math.Round(t.Amount, DisplayScale, DisplayRounding);

                decimal postedDebits = Math.Round((decimal)periodDebits - hiddenDebitTotal,
                    DisplayScale, DisplayRounding);
                decimal postedCredits = Math.Round((decimal)periodCredits - hiddenCreditTotal,
                    DisplayScale, DisplayRounding);

                decimal endingBalance = isDebit
                    ? openingBalance + postedDebits - postedCredits
                    : openingBalance + postedCredits - postedDebits;

                // 8. Calculate running balance for each transaction
                decimal runningBalance = openingBalance;
                foreach (var t in transactions)
                {
                    decimal signed = t.DebitCreditFlag == "D" ? t.Amount : -t.Amount;
                    decimal impact = isDebit ? signed : -signed;
                    runningBalance += impact;
                    t.RunningBalance = Math.Round(runningBalance, DisplayScale, DisplayRounding);
                }

                // 9. Return result
                return Result<GlAccountTransactionDetails>.Success(new GlAccountTransactionDetails
                {
                    OpeningBalance = openingBalance,
                    PostedDebits = postedDebits,
                    PostedCredits = postedCredits,
                    EndingBalance = endingBalance,
                    // REFACTOR (2026-08-14): pass the account side (already computed at step 6
                    // above) through to the response so the frontend Excel export can do its own
                    // debit/credit math correctly instead of assuming every account is debit-natured.
                    IsDebit = isDebit,
                    GlAccountId = request.GlAccountId,
                    AccountCode = glAccount.AccountCode,
                    AccountName = glAccount.AccountNameArabic,
                    Transactions = transactions
                });
            }
            catch (Exception ex)
            {
                return Result<GlAccountTransactionDetails>.Failure(
                    $"Error retrieving transaction details: {ex.Message}");
            }
        }
    }
}