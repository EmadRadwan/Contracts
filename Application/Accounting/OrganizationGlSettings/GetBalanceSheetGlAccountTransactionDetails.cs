using Application.Accounting.Services;
using Application.Accounting.Services.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.OrganizationGlSettings
{
    public class GetBalanceSheetGlAccountTransactionDetails
    {
        public class Query : IRequest<Result<GlAccountTransactionDetails>>
        {
            public string OrganizationPartyId { get; set; }
            public DateTime? ThruDate { get; set; }
            public string GlFiscalTypeId { get; set; }
            public string GlAccountId { get; set; }
            public bool IncludePrePeriodTransactions { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<GlAccountTransactionDetails>>
        {
            private readonly DataContext _context;
            private readonly IAcctgReportsService _acctgReportsService;
            private readonly IAcctgMiscService _acctgMiscService;

            public Handler(DataContext context, IAcctgReportsService acctgReportsService, IAcctgMiscService acctgMiscService)
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
                    {
                        request.ThruDate = DateTime.Now;
                    }

                    // 1. Find the last closed period to get FromDate
                    var lastClosedDateResult = await _acctgReportsService.FindLastClosedDate(request.OrganizationPartyId, request.ThruDate.Value, null);
                    if (lastClosedDateResult?.LastClosedDate == null)
                    {
                        return Result<GlAccountTransactionDetails>.Failure("Could not find a closed period.");
                    }

                    DateTime fromDate = lastClosedDateResult.LastClosedDate.Value;
                    DateTime thruDate = request.ThruDate.Value;

                    // 2. Retrieve GL Account
                    var glAccount = await _context.GlAccounts.FindAsync(request.GlAccountId);
                    if (glAccount == null)
                    {
                        return Result<GlAccountTransactionDetails>.Failure("GlAccount not found.");
                    }

                    // 3. Build query for transactions
                    var query = from ate in _context.AcctgTransEntries
                                join act in _context.AcctgTrans on ate.AcctgTransId equals act.AcctgTransId
                                join att in _context.AcctgTransTypes on act.AcctgTransTypeId equals att.AcctgTransTypeId into transTypes
                                from att in transTypes.DefaultIfEmpty()
                                join p in _context.Parties on act.PartyId equals p.PartyId into parties
                                from p in parties.DefaultIfEmpty()
                                join prod in _context.Products on ate.ProductId equals prod.ProductId into products
                                from prod in products.DefaultIfEmpty()
                                join we in _context.WorkEfforts on act.WorkEffortId equals we.WorkEffortId into workEfforts
                                from we in workEfforts.DefaultIfEmpty()
                                join project in _context.WorkEfforts on we.ProjectId equals project.WorkEffortId into projects
                                from project in projects.DefaultIfEmpty()
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

                    IQueryable<TransactionEntryDto> transactionsQuery;
                    if (request.IncludePrePeriodTransactions)
                    {
                        transactionsQuery = query.Where(x => x.TransactionDate < thruDate);
                    }
                    else
                    {
                        transactionsQuery = query.Where(x => x.TransactionDate >= fromDate && x.TransactionDate < thruDate);
                    }

                    var transactions = await transactionsQuery
                        .OrderBy(x => x.TransactionDate)
                        .ThenBy(x => x.AcctgTransId)
                        .ThenBy(x => x.AcctgTransEntrySeqId)
                        .ToListAsync(cancellationToken);

                    // 4. Calculate aggregates
                    // Opening = everything up to fromDate (which is the last closed date)
                    var openingDebits = await (
                        from ate in _context.AcctgTransEntries
                        join act in _context.AcctgTrans on ate.AcctgTransId equals act.AcctgTransId
                        where ate.OrganizationPartyId == request.OrganizationPartyId
                              && ate.GlAccountId == request.GlAccountId
                              && act.IsPosted == "Y"
                              && ate.DebitCreditFlag == "D"
                              && act.GlFiscalTypeId == request.GlFiscalTypeId
                              && act.TransactionDate < fromDate
                        select ate.Amount
                    ).SumAsync(cancellationToken);

                    var openingCredits = await (
                        from ate in _context.AcctgTransEntries
                        join act in _context.AcctgTrans on ate.AcctgTransId equals act.AcctgTransId
                        where ate.OrganizationPartyId == request.OrganizationPartyId
                              && ate.GlAccountId == request.GlAccountId
                              && act.IsPosted == "Y"
                              && ate.DebitCreditFlag == "C"
                              && act.GlFiscalTypeId == request.GlFiscalTypeId
                              && act.TransactionDate < fromDate
                        select ate.Amount
                    ).SumAsync(cancellationToken);

                    // Ending = everything up to thruDate
                    var endingDebits = await (
                        from ate in _context.AcctgTransEntries
                        join act in _context.AcctgTrans on ate.AcctgTransId equals act.AcctgTransId
                        where ate.OrganizationPartyId == request.OrganizationPartyId
                              && ate.GlAccountId == request.GlAccountId
                              && act.IsPosted == "Y"
                              && ate.DebitCreditFlag == "D"
                              && act.GlFiscalTypeId == request.GlFiscalTypeId
                              && act.TransactionDate < thruDate
                        select ate.Amount
                    ).SumAsync(cancellationToken);

                    var endingCredits = await (
                        from ate in _context.AcctgTransEntries
                        join act in _context.AcctgTrans on ate.AcctgTransId equals act.AcctgTransId
                        where ate.OrganizationPartyId == request.OrganizationPartyId
                              && ate.GlAccountId == request.GlAccountId
                              && act.IsPosted == "Y"
                              && ate.DebitCreditFlag == "C"
                              && act.GlFiscalTypeId == request.GlFiscalTypeId
                              && act.TransactionDate < thruDate
                        select ate.Amount
                    ).SumAsync(cancellationToken);

                    bool isDebit = await _acctgMiscService.IsDebitAccount(request.GlAccountId);

                    decimal openingBalance = (decimal)(isDebit
                        ? openingDebits - openingCredits
                        : openingCredits - openingDebits);

                    decimal endingBalance = (decimal)(isDebit
                        ? endingDebits - endingCredits
                        : endingCredits - endingDebits);

                    decimal postedDebits = (decimal)(endingDebits - openingDebits);
                    decimal postedCredits = (decimal)(endingCredits - openingCredits);

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
                    return Result<GlAccountTransactionDetails>.Failure($"Error: {ex.Message}");
                }
            }
        }
    }
}
