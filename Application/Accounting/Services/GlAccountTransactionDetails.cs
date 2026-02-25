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
                    where ate.OrganizationPartyId == request.OrganizationPartyId
                          && ate.GlAccountId == request.GlAccountId
                          && act.IsPosted == "Y"
                          && act.GlFiscalTypeId == "ACTUAL"
                    select new TransactionEntryDto
                    {
                        AcctgTransId = ate.AcctgTransId,
                        AcctgTransEntrySeqId = ate.AcctgTransEntrySeqId,
                        TransactionDate = (DateTime)act.TransactionDate,
                        AcctgTransTypeId = act.AcctgTransTypeId ?? "Unknown",
                        AcctgTransTypeDescription = att != null ? att.Description : (act.AcctgTransTypeId ?? "Unknown"),
                        GlFiscalTypeId = act.GlFiscalTypeId,
                        InvoiceId = act.InvoiceId,
                        PaymentId = act.PaymentId,
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
                        Description = act.Description,
                        CurrencyUomId = ate.CurrencyUomId,
                        CertificateNumber = we != null ? we.CertificateNumber : null,
                        ProjectName = we != null
                            ? (we.WorkEffortTypeId == "PROJECT" 
                                ? (we.ProjectName ?? we.Description ?? we.WorkEffortName)
                                : (project != null ? (project.ProjectName ?? project.Description ?? project.WorkEffortName) : null))
                            : null,
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
                    .ThenBy(x => x.AcctgTransEntrySeqId)
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
                          && act.TransactionDate <= openingCutoff
                    select ate.Amount
                ).SumAsync(cancellationToken);

                // Ending = everything up to end of period (≤ ThruDate)
                var endingDebits = await (
                    from ate in _context.AcctgTransEntries
                    join act in _context.AcctgTrans on ate.AcctgTransId equals act.AcctgTransId
                    where ate.OrganizationPartyId == request.OrganizationPartyId
                          && ate.GlAccountId == request.GlAccountId
                          && act.IsPosted == "Y"
                          && ate.DebitCreditFlag == "D"
                          && act.GlFiscalTypeId == "ACTUAL"
                          && act.TransactionDate <= periodEnd
                    select ate.Amount
                ).SumAsync(cancellationToken);

                var endingCredits = await (
                    from ate in _context.AcctgTransEntries
                    join act in _context.AcctgTrans on ate.AcctgTransId equals act.AcctgTransId
                    where ate.OrganizationPartyId == request.OrganizationPartyId
                          && ate.GlAccountId == request.GlAccountId
                          && act.IsPosted == "Y"
                          && ate.DebitCreditFlag == "C"
                          && act.GlFiscalTypeId == "ACTUAL"
                          && act.TransactionDate <= periodEnd
                    select ate.Amount
                ).SumAsync(cancellationToken);

                // 6. Determine account side
                bool isDebit = await _acctgMiscService.IsDebitAccount(request.GlAccountId);

                // 7. Calculate balances
                decimal openingBalance = (decimal)(isDebit
                    ? openingDebits - openingCredits
                    : openingCredits - openingDebits);

                decimal endingBalance = (decimal)(isDebit
                    ? endingDebits - endingCredits
                    : endingCredits - endingDebits);

                decimal postedDebits = (decimal)(endingDebits - openingDebits);
                decimal postedCredits = (decimal)(endingCredits - openingCredits);

                // 8. Return result
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