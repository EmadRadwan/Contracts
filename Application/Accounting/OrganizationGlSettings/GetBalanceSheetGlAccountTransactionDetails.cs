using Application.Accounting.Services;
using Application.Accounting.Services.Models;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.OrganizationGlSettings
{
    public class GetBalanceSheetGlAccountTransactionDetails
    {
        public class Query : IRequest<Result<GlAccountTransactionDetails>>
        {
            public string OrganizationPartyId { get; set; } = string.Empty;
            public DateTime? ThruDate { get; set; }
            public string GlFiscalTypeId { get; set; } = string.Empty;
            public string GlAccountId { get; set; } = string.Empty;
            public bool IncludePrePeriodTransactions { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<GlAccountTransactionDetails>>
        {
            private readonly DataContext _context;
            private readonly IAcctgReportsService _acctgReportsService;
            private readonly IAcctgMiscService _acctgMiscService;

            public Handler(
                DataContext context, 
                IAcctgReportsService acctgReportsService, 
                IAcctgMiscService acctgMiscService)
            {
                _context = context;
                _acctgReportsService = acctgReportsService;
                _acctgMiscService = acctgMiscService;
            }

            public async Task<Result<GlAccountTransactionDetails>> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {
                    if (!request.ThruDate.HasValue)
                        request.ThruDate = DateTime.UtcNow;

                    // 1. Get the same reporting period as the main Balance Sheet
                    var lastClosedResult = await _acctgReportsService.FindLastClosedDate(
                        request.OrganizationPartyId, 
                        request.ThruDate.Value, 
                        null);

                    if (lastClosedResult?.LastClosedDate == null)
                        return Result<GlAccountTransactionDetails>.Failure("Could not determine reporting period.");

                    DateTime fromDate = lastClosedResult.LastClosedDate.Value;
                    var lastClosedTimePeriod = lastClosedResult.LastClosedTimePeriod;

                    // 2. Get GL Account basic info
                    var glAccount = await _context.GlAccounts.FindAsync(request.GlAccountId);
                    if (glAccount == null)
                        return Result<GlAccountTransactionDetails>.Failure("GlAccount not found.");

                    // 3. Determine debit/credit nature of the account
                    bool isDebitAccount = await _acctgMiscService.IsDebitAccount(request.GlAccountId);

                    // 4. Calculate Opening Balance (Fully consistent with GetOpeningBalances logic)
                    decimal openingBalance = await CalculateOpeningBalance(
                        request.OrganizationPartyId,
                        request.GlAccountId,
                        lastClosedTimePeriod,
                        isDebitAccount);

                    // 5. Build detailed transactions query
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
                        where ate.OrganizationPartyId == request.OrganizationPartyId
                              && ate.GlAccountId == request.GlAccountId
                              && act.IsPosted == "Y"
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
                            PostedDate = act.PostedDate,
                            // Add any other fields you need for the modal
                        };

                    // Filter transactions based on "Include Pre-Period" checkbox
                    if (!request.IncludePrePeriodTransactions)
                    {
                        // Only transactions within the current reporting period
                        transactionsQuery = transactionsQuery.Where(x => 
                            x.TransactionDate >= fromDate && x.TransactionDate < request.ThruDate.Value);
                    }
                    else
                    {
                        // All transactions before thruDate (including pre-period)
                        transactionsQuery = transactionsQuery.Where(x => 
                            x.TransactionDate < request.ThruDate.Value);
                    }

                    var transactions = await transactionsQuery
                        .OrderBy(x => x.TransactionDate)
                        .ThenBy(x => x.AcctgTransId)
                        .ThenBy(x => x.AcctgTransEntrySeqId)
                        .ToListAsync(cancellationToken);

                    // 6. Calculate period movements and ending balance
                    decimal periodDebits = transactions.Where(t => t.DebitCreditFlag == "D").Sum(t => t.Amount);
                    decimal periodCredits = transactions.Where(t => t.DebitCreditFlag == "C").Sum(t => t.Amount);

                    decimal endingBalance = isDebitAccount 
                        ? openingBalance + periodDebits - periodCredits 
                        : openingBalance + periodCredits - periodDebits;

                    return Result<GlAccountTransactionDetails>.Success(new GlAccountTransactionDetails
                    {
                        GlAccountId = request.GlAccountId,
                        AccountCode = glAccount.AccountCode,
                        AccountName = glAccount.AccountNameArabic ?? glAccount.AccountName,
                        OpeningBalance = openingBalance,
                        PostedDebits = periodDebits,
                        PostedCredits = periodCredits,
                        EndingBalance = endingBalance,
                        Transactions = transactions
                    });
                }
                catch (Exception ex)
                {
                    return Result<GlAccountTransactionDetails>.Failure($"Error retrieving transaction details: {ex.Message}");
                }
            }

            // ===================================================================
            // Helper: Opening Balance Calculation - 100% consistent with GetOpeningBalances
            // ===================================================================
            private async Task<decimal> CalculateOpeningBalance(
                string organizationPartyId,
                string glAccountId,
                CustomTimePeriod? lastClosedTimePeriod,
                bool isDebitAccount)
            {
                if (lastClosedTimePeriod == null)
                    return 0m;

                // Priority 1: Try GlAccountHistory first (same as main Balance Sheet)
                var historyBalance = await (
                    from glah in _context.GlAccountHistories
                    where glah.OrganizationPartyId == organizationPartyId
                          && glah.GlAccountId == glAccountId
                          && glah.CustomTimePeriodId == lastClosedTimePeriod.CustomTimePeriodId
                    select glah.EndingBalance
                ).FirstOrDefaultAsync();

                if (historyBalance != null && historyBalance != 0)
                    return (decimal)historyBalance;

                // Priority 2: Fallback to OPENING_BALANCE transactions (when no history exists yet)
                if (!lastClosedTimePeriod.FromDate.HasValue)
                    return 0m;

                var openingCutoff = lastClosedTimePeriod.FromDate.Value.Date.AddTicks(-1);

                var openingTx = await (
                    from ate in _context.AcctgTransEntries
                    join act in _context.AcctgTrans on ate.AcctgTransId equals act.AcctgTransId
                    where ate.OrganizationPartyId == organizationPartyId
                          && ate.GlAccountId == glAccountId
                          && act.IsPosted == "Y"
                          && act.AcctgTransTypeId == "OPENING_BALANCE"
                          && act.GlFiscalTypeId == "ACTUAL"
                          && act.TransactionDate <= openingCutoff
                    group ate by ate.DebitCreditFlag into g
                    select new
                    {
                        Debit = g.Key == "D" ? g.Sum(x => x.Amount) : 0m,
                        Credit = g.Key == "C" ? g.Sum(x => x.Amount) : 0m
                    }
                ).FirstOrDefaultAsync();

                decimal totalDebit = openingTx?.Debit ?? 0m;
                decimal totalCredit = openingTx?.Credit ?? 0m;

                return isDebitAccount 
                    ? totalDebit - totalCredit 
                    : totalCredit - totalDebit;
            }
        }
    }
}