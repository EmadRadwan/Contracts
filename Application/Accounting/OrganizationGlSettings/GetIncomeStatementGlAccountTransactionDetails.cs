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

                    // 3. Opening Balance for Income Statement is usually 0 if we start from the beginning of the period,
                    // but if includePrePeriod is false, we might want "opening" as transactions before fromDate.
                    // However, Income Statement accounts are temporary.
                    // For the sake of the "Details" modal, let's calculate transactions before fromDate as "Opening" if includePrePeriod is false.
                    
                    var baseQuery = from ate in _context.AcctgTransEntries
                        join act in _context.AcctgTrans on ate.AcctgTransId equals act.AcctgTransId
                        where ate.OrganizationPartyId == request.OrganizationPartyId
                              && ate.GlAccountId == request.GlAccountId
                              && act.IsPosted == "Y"
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

                    return Result<GlAccountTransactionDetails>.Success(new GlAccountTransactionDetails
                    {
                        GlAccountId = request.GlAccountId,
                        AccountCode = glAccount.AccountCode,
                        AccountName = glAccount.AccountNameArabic ?? glAccount.AccountName,
                        OpeningBalance = openingBalance,
                        PostedDebits = periodDebits,
                        PostedCredits = periodCredits,
                        EndingBalance = endingBalance,
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
