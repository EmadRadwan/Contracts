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

                // 3. Build query for transactions
                var query = from ate in _context.AcctgTransEntries
                    join act in _context.AcctgTrans on ate.AcctgTransId equals act.AcctgTransId
                    join p in _context.Parties on ate.PartyId equals p.PartyId into parties
                    from p in parties.DefaultIfEmpty()
                    join prod in _context.Products on ate.ProductId equals prod.ProductId into products
                    from prod in products.DefaultIfEmpty()
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
                        GlFiscalTypeId = act.GlFiscalTypeId,
                        InvoiceId = act.InvoiceId,
                        PaymentId = act.PaymentId,
                        WorkEffortId = act.WorkEffortId,
                        ShipmentId = act.ShipmentId,
                        PartyId = ate.PartyId,
                        PartyName = p != null ? p.Description : null,
                        ProductId = ate.ProductId,
                        ProductName = prod != null ? prod.ProductName : null,
                        IsPosted = act.IsPosted,
                        PostedDate = act.PostedDate,
                        DebitCreditFlag = ate.DebitCreditFlag,
                        Amount = (decimal)ate.Amount,
                        Description = ate.Description,
                        CurrencyUomId = ate.CurrencyUomId
                    };

                // 4. Filter transactions based on includePrePeriodTransactions
                var transactionsQuery = request.IncludePrePeriodTransactions
                    ? query.Where(x => x.TransactionDate < customTimePeriod.ThruDate)
                    : query.Where(x => x.TransactionDate >= customTimePeriod.FromDate
                                       && x.TransactionDate < customTimePeriod.ThruDate);

                // 5. Execute query and sort by date
                var transactions = await transactionsQuery
                    .OrderBy(x => x.TransactionDate)
                    .ToListAsync(cancellationToken);

                // 6. Calculate balances
                var openingDebits = request.IncludePrePeriodTransactions
                    ? (decimal)await (from ate in _context.AcctgTransEntries
                        join act in _context.AcctgTrans on ate.AcctgTransId equals act.AcctgTransId
                        where ate.OrganizationPartyId == request.OrganizationPartyId
                              && ate.GlAccountId == request.GlAccountId
                              && act.IsPosted == "Y"
                              && ate.DebitCreditFlag == "D"
                              && act.GlFiscalTypeId == "ACTUAL"
                              && act.TransactionDate < customTimePeriod.FromDate
                        select ate.Amount).SumAsync(cancellationToken)
                    : 0;

                var openingCredits = request.IncludePrePeriodTransactions
                    ? (decimal)await (from ate in _context.AcctgTransEntries
                        join act in _context.AcctgTrans on ate.AcctgTransId equals act.AcctgTransId
                        where ate.OrganizationPartyId == request.OrganizationPartyId
                              && ate.GlAccountId == request.GlAccountId
                              && act.IsPosted == "Y"
                              && ate.DebitCreditFlag == "C"
                              && act.GlFiscalTypeId == "ACTUAL"
                              && act.TransactionDate < customTimePeriod.FromDate
                        select ate.Amount).SumAsync(cancellationToken)
                    : 0;

                var endingDebits = (decimal)await (from ate in _context.AcctgTransEntries
                    join act in _context.AcctgTrans on ate.AcctgTransId equals act.AcctgTransId
                    where ate.OrganizationPartyId == request.OrganizationPartyId
                          && ate.GlAccountId == request.GlAccountId
                          && act.IsPosted == "Y"
                          && ate.DebitCreditFlag == "D"
                          && act.GlFiscalTypeId == "ACTUAL"
                          && act.TransactionDate < customTimePeriod.ThruDate
                    select ate.Amount).SumAsync(cancellationToken);

                var endingCredits = (decimal)await (from ate in _context.AcctgTransEntries
                    join act in _context.AcctgTrans on ate.AcctgTransId equals act.AcctgTransId
                    where ate.OrganizationPartyId == request.OrganizationPartyId
                          && ate.GlAccountId == request.GlAccountId
                          && act.IsPosted == "Y"
                          && ate.DebitCreditFlag == "C"
                          && act.GlFiscalTypeId == "ACTUAL"
                          && act.TransactionDate < customTimePeriod.ThruDate
                    select ate.Amount).SumAsync(cancellationToken);

                // 7. Determine if debit account
                bool isDebit = await _acctgMiscService.IsDebitAccount(request.GlAccountId);

                // 8. Calculate opening and ending balances
                decimal openingBalance = isDebit
                    ? openingDebits - openingCredits
                    : openingCredits - openingDebits;

                decimal endingBalance = isDebit
                    ? endingDebits - endingCredits
                    : endingCredits - endingDebits;

                // 9. Calculate period debits and credits
                decimal postedDebits = endingDebits - openingDebits;
                decimal postedCredits = endingCredits - openingCredits;

                // 10. Return result
                return Result<GlAccountTransactionDetails>.Success(new GlAccountTransactionDetails
                {
                    OpeningBalance = openingBalance,
                    PostedDebits = postedDebits,
                    PostedCredits = postedCredits,
                    EndingBalance = endingBalance,
                    GlAccountId = request.GlAccountId,
                    AccountCode = glAccount.AccountCode,
                    AccountName = glAccount.AccountName,
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