using Application.Accounting.Services;
using Application.Accounting.Services.Models;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.OrganizationGlSettings
{
    public class GetIncomeStatementGlAccountTransactionDetails
    {
        public class Query : IRequest<Result<GlAccountTransactionDetails>>
        {
            public string OrganizationPartyId { get; set; } = string.Empty;
            public DateTime? FromDate { get; set; }
            public DateTime? ThruDate { get; set; }
            public int? SelectedMonth { get; set; }
            public string GlFiscalTypeId { get; set; } = string.Empty;
            public string GlAccountId { get; set; } = string.Empty;
            public bool IncludePrePeriodTransactions { get; set; }

            // "Y" / "N" to restrict, null or "ALL" for both. The drill-down used to hard-filter
            // to posted entries, which made it useless behind any report that also shows
            // unposted figures (Transaction Totals has posted / unposted / all tabs) — the modal
            // simply came up empty on those.
            public string? IsPosted { get; set; }
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

            public async Task<Result<GlAccountTransactionDetails>> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {
                    DateTime? fromDate = request.FromDate;
                    DateTime? thruDate = request.ThruDate;

                    if (request.SelectedMonth.HasValue)
                    {
                        var year = thruDate?.Year ?? DateTime.UtcNow.Year;
                        fromDate = new DateTime(year, request.SelectedMonth.Value + 1, 1);
                        thruDate = fromDate.Value.AddMonths(1).AddTicks(-1);
                    }

                    if (!thruDate.HasValue) thruDate = DateTime.UtcNow;
                    if (!fromDate.HasValue) fromDate = new DateTime(thruDate.Value.Year, 1, 1);

                    // 1. Get GL Account basic info
                    var glAccount = await _context.GlAccounts.FindAsync(request.GlAccountId);
                    if (glAccount == null)
                        return Result<GlAccountTransactionDetails>.Failure("GlAccount not found.");

                    // 2. Determine debit/credit nature of the account
                    bool isDebitAccount = await _acctgMiscService.IsDebitAccount(request.GlAccountId);

                    // Null means "no restriction"; anything else is matched exactly. Defaults to
                    // posted-only so every existing caller keeps its current behaviour.
                    string? postedFilter =
                        string.IsNullOrWhiteSpace(request.IsPosted) ? "Y"
                        : request.IsPosted.Equals("ALL", StringComparison.OrdinalIgnoreCase) ? null
                        : request.IsPosted;

                    // 3. Opening Balance for Income Statement is usually 0 if we start from the beginning of the period,
                    // but if includePrePeriod is false, we might want "opening" as transactions before fromDate.
                    // However, Income Statement accounts are temporary.
                    // For the sake of the "Details" modal, let's calculate transactions before fromDate as "Opening" if includePrePeriod is false.
                    
                    var baseQuery = from ate in _context.AcctgTransEntries
                        join act in _context.AcctgTrans on ate.AcctgTransId equals act.AcctgTransId
                        where ate.OrganizationPartyId == request.OrganizationPartyId
                              && ate.GlAccountId == request.GlAccountId
                              && (postedFilter == null || act.IsPosted == postedFilter)
                              && act.GlFiscalTypeId == request.GlFiscalTypeId
                        select new { ate, act };

                    decimal openingBalance = 0;
                    if (!request.IncludePrePeriodTransactions)
                    {
                        var openingTx = await baseQuery
                            .Where(x => x.act.TransactionDate < fromDate)
                            .GroupBy(x => x.ate.DebitCreditFlag)
                            .Select(g => new
                            {
                                Flag = g.Key,
                                Amount = g.Sum(x => x.ate.Amount)
                            })
                            .ToListAsync(cancellationToken);

                        decimal totalDebit = (decimal)(openingTx.FirstOrDefault(x => x.Flag == "D")?.Amount ?? 0);
                        decimal totalCredit = (decimal)(openingTx.FirstOrDefault(x => x.Flag == "C")?.Amount ?? 0);

                        openingBalance = isDebitAccount ? totalDebit - totalCredit : totalCredit - totalDebit;
                    }

                    // 4. Detailed transactions
                    var transactionsQuery = from ate in _context.AcctgTransEntries
                        join act in _context.AcctgTrans on ate.AcctgTransId equals act.AcctgTransId
                        join att in _context.AcctgTransTypes on act.AcctgTransTypeId equals att.AcctgTransTypeId into transTypes
                        from att in transTypes.DefaultIfEmpty()
                        join p in _context.Parties on act.PartyId equals p.PartyId into parties
                        from p in parties.DefaultIfEmpty()
                        join prod in _context.Products on ate.ProductId equals prod.ProductId into products
                        from prod in products.DefaultIfEmpty()
                        join we in _context.WorkEfforts on act.WorkEffortId equals we.WorkEffortId into workEfforts
                        from we in workEfforts.DefaultIfEmpty()
                        // Payment -> cost center -> ref num, same chain the trial balance drill-down
                        // uses. All are left joins on primary keys, so no row is multiplied; they only
                        // fill columns the shared PDF/Excel exports print.
                        join project in _context.WorkEfforts on we.ProjectId equals project.WorkEffortId into projects
                        from project in projects.DefaultIfEmpty()
                        join pyt in _context.Payments on act.PaymentId equals pyt.PaymentId into payments
                        from pyt in payments.DefaultIfEmpty()
                        join cc in _context.CostCenters on pyt.CostCenterId equals cc.CostCenterId into costCenters
                        from cc in costCenters.DefaultIfEmpty()
                        where ate.OrganizationPartyId == request.OrganizationPartyId
                              && ate.GlAccountId == request.GlAccountId
                              && (postedFilter == null || act.IsPosted == postedFilter)
                              && act.GlFiscalTypeId == request.GlFiscalTypeId
                        select new TransactionEntryDto
                        {
                            AcctgTransId = ate.AcctgTransId,
                            AcctgTransEntrySeqId = ate.AcctgTransEntrySeqId,
                            TransactionDate = (DateTime)act.TransactionDate,
                            AcctgTransTypeId = act.AcctgTransTypeId ?? "Unknown",
                            AcctgTransTypeDescription = att != null ? att.Description : (act.AcctgTransTypeId ?? "Unknown"),
                            DebitCreditFlag = ate.DebitCreditFlag,
                            Amount = (decimal)ate.Amount,
                            Description = act.Description,
                            PartyName = p != null ? p.Description : null,
                            ProductName = prod != null ? prod.ProductName : null,
                            CertificateNumber = we != null ? we.CertificateNumber : null,
                            IsPosted = act.IsPosted,
                            ReversalOfAcctgTransId = _context.AcctgTransAttributes.Where(a => a.AcctgTransId == act.AcctgTransId && a.AttrName == "REVERSAL_OF").Select(a => a.AttrValue).FirstOrDefault(),
                            ReversedByAcctgTransId = _context.AcctgTransAttributes.Where(a => a.AcctgTransId == act.AcctgTransId && a.AttrName == "REVERSED_BY").Select(a => a.AttrValue).FirstOrDefault(),
                            PostedDate = act.PostedDate,
                            InvoiceId = act.InvoiceId,
                            PaymentId = we != null && we.WorkEffortTypeId == "PAYMENT_CERTIFICATE"
                                ? we.WorkEffortId
                                : act.PaymentId,
                            WorkEffortId = act.WorkEffortId,
                            ShipmentId = act.ShipmentId,
                            PartyId = act.PartyId,
                            ProductId = ate.ProductId,
                            CurrencyUomId = ate.CurrencyUomId,
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

                    if (!request.IncludePrePeriodTransactions)
                    {
                        transactionsQuery = transactionsQuery.Where(x => x.TransactionDate >= fromDate && x.TransactionDate <= thruDate);
                    }
                    else
                    {
                        transactionsQuery = transactionsQuery.Where(x => x.TransactionDate <= thruDate);
                    }

                    var transactions = await transactionsQuery
                        .OrderBy(x => x.TransactionDate)
                        .ThenBy(x => x.AcctgTransId)
                        .ThenBy(x => x.AcctgTransEntrySeqId)
                        .ToListAsync(cancellationToken);

                    // 5. Calculate period movements and ending balance
                    decimal periodDebits = transactions.Where(t => t.DebitCreditFlag == "D").Sum(t => t.Amount);
                    decimal periodCredits = transactions.Where(t => t.DebitCreditFlag == "C").Sum(t => t.Amount);

                    decimal endingBalance = isDebitAccount 
                        ? openingBalance + periodDebits - periodCredits 
                        : openingBalance + periodCredits - periodDebits;

                    // 6. Running balance per transaction — same walk as the trial balance drill-down
                    // (GetGlAccountTransactionDetails step 8). The PDF export prints this column, and
                    // it is left at 0 on every row without it.
                    decimal runningBalance = openingBalance;
                    foreach (var t in transactions)
                    {
                        decimal signed = t.DebitCreditFlag == "D" ? t.Amount : -t.Amount;
                        runningBalance += isDebitAccount ? signed : -signed;
                        t.RunningBalance = runningBalance;
                    }

                    // 7. Period identity for the PDF's running header. An income statement drill-down
                    // is an ad-hoc date range (or a selected month), not a named CustomTimePeriod, so
                    // only the dates are supplied and PeriodName is left null — the report prints the
                    // bare range in that case. With "include pre-period" on the listing reaches back
                    // before fromDate, so the header shows the span actually covered.
                    var organizationName = await _context.PartyGroups
                        .Where(pg => pg.PartyId == request.OrganizationPartyId)
                        .Select(pg => pg.GroupName)
                        .FirstOrDefaultAsync(cancellationToken);

                    return Result<GlAccountTransactionDetails>.Success(new GlAccountTransactionDetails
                    {
                        OrganizationName = organizationName ?? request.OrganizationPartyId,
                        PeriodFromDate = request.IncludePrePeriodTransactions
                            ? (transactions.Count > 0 ? transactions[0].TransactionDate : fromDate)
                            : fromDate,
                        PeriodThruDate = thruDate,
                        GlAccountId = request.GlAccountId,
                        AccountCode = glAccount.AccountCode,
                        AccountName = glAccount.AccountNameArabic ?? glAccount.AccountName,
                        OpeningBalance = openingBalance,
                        PostedDebits = periodDebits,
                        PostedCredits = periodCredits,
                        EndingBalance = endingBalance,
                        // REFACTOR (2026-08-14): expose the account side already computed above
                        // (isDebitAccount) so the frontend date-range Excel export can roll opening/
                        // running balances up correctly for credit-natured accounts too — see
                        // GlAccountTransactionsDateRangeExcel.tsx for the consumer of this field.
                        IsDebit = isDebitAccount,
                        Transactions = transactions
                    });
                }
                catch (Exception ex)
                {
                    return Result<GlAccountTransactionDetails>.Failure($"Error retrieving transaction details: {ex.Message}");
                }
            }
        }
    }
}
