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
                var openingCutoff = periodStart.AddTicks(-1);

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

                // 5. Calculate aggregates using consistent cutoffs
                // Opening = everything BEFORE the period starts (≤ day before FromDate)
                var openingDebits = await (
                    from ate in _context.AcctgTransEntries
                    join act in _context.AcctgTrans on ate.AcctgTransId equals act.AcctgTransId
                    where ate.OrganizationPartyId == request.OrganizationPartyId
                          && ate.GlAccountId == request.GlAccountId
                          && act.IsPosted == "Y"
                          && ate.DebitCreditFlag == "D"
                          && act.GlFiscalTypeId == "ACTUAL"
                      && act.AcctgTransTypeId == "OPENING_BALANCE"
                          && act.TransactionDate <= openingCutoff
                    select ate.Amount
                ).SumAsync(cancellationToken);

                var openingCredits = await (
                    from ate in _context.AcctgTransEntries
                    join act in _context.AcctgTrans on ate.AcctgTransId equals act.AcctgTransId
                    where ate.OrganizationPartyId == request.OrganizationPartyId
                          && ate.GlAccountId == request.GlAccountId
                          && act.IsPosted == "Y"
                          && ate.DebitCreditFlag == "C"
                          && act.GlFiscalTypeId == "ACTUAL"
                      && act.AcctgTransTypeId == "OPENING_BALANCE"
                          && act.TransactionDate <= openingCutoff
                    select ate.Amount
                ).SumAsync(cancellationToken);

                // Accounts that have never had a proper OPENING_BALANCE reset entry (e.g. customer
                // receivable sub-accounts) are not meant to be period-bounded: their opening balance
                // is always 0, so bounding period activity to >= periodStart would zero out real
                // pre-period history and make the account look like it started fresh this period.
                // For those, fall back to the lifetime-to-date total (bounded only by periodEnd),
                // matching how they've always been used. Accounts with a real reset entry (banks,
                // cash) get the strict [periodStart, periodEnd] bound.
                bool hasOpeningBalanceEntry = await (
                    from ate in _context.AcctgTransEntries
                    join act in _context.AcctgTrans on ate.AcctgTransId equals act.AcctgTransId
                    where ate.OrganizationPartyId == request.OrganizationPartyId
                          && ate.GlAccountId == request.GlAccountId
                          && act.IsPosted == "Y"
                          && act.GlFiscalTypeId == "ACTUAL"
                          && act.AcctgTransTypeId == "OPENING_BALANCE"
                    select ate.AcctgTransId
                ).AnyAsync(cancellationToken);

                var periodDebits = await (
                    from ate in _context.AcctgTransEntries
                    join act in _context.AcctgTrans on ate.AcctgTransId equals act.AcctgTransId
                    where ate.OrganizationPartyId == request.OrganizationPartyId
                          && ate.GlAccountId == request.GlAccountId
                          && act.IsPosted == "Y"
                          && ate.DebitCreditFlag == "D"
                          && act.GlFiscalTypeId == "ACTUAL"
                          && (!hasOpeningBalanceEntry || act.TransactionDate >= periodStart)
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
                          && (!hasOpeningBalanceEntry || act.TransactionDate >= periodStart)
                          && act.TransactionDate <= periodEnd
                    select ate.Amount
                ).SumAsync(cancellationToken);

                // 6. Determine account side
                bool isDebit = await _acctgMiscService.IsDebitAccount(request.GlAccountId);

                // 7. Calculate balances
                decimal openingBalance = (decimal)(isDebit
                    ? openingDebits - openingCredits
                    : openingCredits - openingDebits);

                decimal postedDebits = (decimal)periodDebits;
                decimal postedCredits = (decimal)periodCredits;

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
                    t.RunningBalance = Math.Round(runningBalance, 2, MidpointRounding.AwayFromZero);
                }

                // 9. Return result
                return Result<GlAccountTransactionDetails>.Success(new GlAccountTransactionDetails
                {
                    OpeningBalance = openingBalance,
                    PostedDebits = postedDebits,
                    PostedCredits = postedCredits,
                    EndingBalance = endingBalance,
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