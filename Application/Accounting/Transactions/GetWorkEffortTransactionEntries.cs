using API.Controllers.Accounting.Transactions;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.Transactions;

public class GetWorkEffortTransactionEntries
{
    public class Query : IRequest<Result<List<AcctgTransEntryDto>>>
    {
        public string WorkEffortId { get; set; } = null!;
        public string Language { get; set; } = null!;
    }

    public class Handler : IRequestHandler<Query, Result<List<AcctgTransEntryDto>>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper  = mapper;
        }

        public async Task<Result<List<AcctgTransEntryDto>>> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            try
            {
                var language = request.Language?.ToLower() ?? "en";

                var entries = await _context.AcctgTransEntries
                    .Join(_context.AcctgTrans,
                        entry => entry.AcctgTransId,
                        trans => trans.AcctgTransId,
                        (entry, trans) => new { AcctgTransEntry = entry, AcctgTrans = trans })
                    .GroupJoin(_context.Products,
                        joined => joined.AcctgTransEntry.ProductId,
                        prod => prod.ProductId,
                        (joined, products) => new { joined, Products = products.DefaultIfEmpty() })
                    .Where(x => x.joined.AcctgTrans.WorkEffortId == request.WorkEffortId)
                    .Select(x => new AcctgTransEntryDto
                    {
                        AcctgTransId              = x.joined.AcctgTransEntry.AcctgTransId,
                        AcctgTransEntrySeqId      = x.joined.AcctgTransEntry.AcctgTransEntrySeqId,
                        AcctgTransTypeDescription = x.joined.AcctgTrans.AcctgTransType.Description,
                        GlAccountTypeDescription  = x.joined.AcctgTransEntry.GlAccount != null
                            ? (language == "en"
                                ? x.joined.AcctgTransEntry.GlAccount.AccountName
                                : x.joined.AcctgTransEntry.GlAccount.AccountNameArabic ?? x.joined.AcctgTransEntry.GlAccount.AccountName)
                            : "N/A",
                        GlAccountClassDescription = x.joined.AcctgTransEntry.GlAccount.GlAccountClass.Description ?? "N/A",
                        GlAccountId               = x.joined.AcctgTransEntry.GlAccountId,
                        Amount                    = x.joined.AcctgTransEntry.Amount,
                        OrigAmount                = x.joined.AcctgTransEntry.OrigAmount,
                        OrigCurrencyUomId         = x.joined.AcctgTransEntry.OrigCurrencyUomId,
                        DebitCreditFlag           = x.joined.AcctgTransEntry.DebitCreditFlag,
                        ProductName               = x.Products.FirstOrDefault().ProductName,
                        IsPosted                  = x.joined.AcctgTrans.IsPosted,
                        GlFiscalTypeId            = x.joined.AcctgTrans.GlFiscalTypeId,
                        TransactionDate           = x.joined.AcctgTrans.TransactionDate,
                        PostedDate                = x.joined.AcctgTrans.PostedDate,
                        // ... add other fields you need (Description, PartyId, etc.)
                    })
                    .ToListAsync(cancellationToken);

                return Result<List<AcctgTransEntryDto>>.Success(entries);
            }
            catch (Exception ex)
            {
                return Result<List<AcctgTransEntryDto>>.Failure(ex.Message);
            }
        }
    }
}