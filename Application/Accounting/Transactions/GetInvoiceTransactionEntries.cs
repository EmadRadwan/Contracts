using API.Controllers.Accounting.Transactions;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Shipments.Transactions
{
    public class GetInvoiceTransactionEntries
    {
        public class Query : IRequest<Result<List<AcctgTransEntryDto>>>
        {
            public string InvoiceId { get; set; }
            public string AcctgTransTypeId { get; set; }
            public string Language { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<List<AcctgTransEntryDto>>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            // REFACTOR: Add IMapper dependency to align with GetPaymentTransactionEntries
            // Ensures consistent mapping behavior if needed for DTO transformations
            public Handler(DataContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<Result<List<AcctgTransEntryDto>>> Handle(Query request,
                CancellationToken cancellationToken)
            {
                try
                {
                    // REFACTOR: Normalize language input to lowercase and default to "en"
                    // Ensures consistent language handling and prevents null reference issues
                    var language = request.Language?.ToLower() ?? "en";

                    var query =
                        from ate in _context.AcctgTransEntries
                        join at in _context.AcctgTrans
                            on ate.AcctgTransId equals at.AcctgTransId // inner join - usually safe
                        join att in _context.AcctgTransTypes
                            on at.AcctgTransTypeId equals att.AcctgTransTypeId into attGroup
                        from att in attGroup.DefaultIfEmpty() // LEFT JOIN AcctgTransType
                        join p in _context.Products
                            on ate.ProductId equals p.ProductId into pGroup
                        from p in pGroup.DefaultIfEmpty() // LEFT JOIN Product
                        join ga in _context.GlAccounts
                            on ate.GlAccountId equals ga.GlAccountId into gaGroup
                        from ga in gaGroup.DefaultIfEmpty() // LEFT JOIN GlAccount
                        where at.InvoiceId == request.InvoiceId
                              && at.AcctgTransTypeId == request.AcctgTransTypeId
                        let lang = (request.Language ?? "en").ToLower()
                        select new AcctgTransEntryDto
                        {
                            AcctgTransId = ate.AcctgTransId,
                            AcctgTransEntrySeqId = ate.AcctgTransEntrySeqId,
                            AcctgTransTypeDescription = att != null ? att.Description : null,
                            Description = ate.Description,
                            PartyId = ate.PartyId,
                            ProductId = ate.ProductId,
                            GlAccountTypeId = ate.GlAccountTypeId,
                            GlAccountId = ate.GlAccountId,
                            OrganizationPartyId = ate.OrganizationPartyId,

                            GlAccountTypeDescription = ga != null
                                ? (lang == "ar" ? ga.AccountNameArabic ?? ga.AccountName : ga.AccountName)
                                : "N/A",

                            Amount = ate.Amount,
                            CurrencyUomId = ate.CurrencyUomId,
                            OrigAmount = ate.OrigAmount,
                            OrigCurrencyUomId = ate.OrigCurrencyUomId,
                            DebitCreditFlag = ate.DebitCreditFlag,
                            DueDate = ate.DueDate,

                            IsPosted = at.IsPosted,
                            TransactionDate = at.TransactionDate,
                            PostedDate = at.PostedDate,

                            ProductName = p != null ? p.ProductName : null
                        };

                    var invoiceTransactionEntries = await query.ToListAsync(cancellationToken);

                    return Result<List<AcctgTransEntryDto>>.Success(invoiceTransactionEntries);
                }
                catch (Exception ex)
                {
                    return Result<List<AcctgTransEntryDto>>.Failure(ex.Message);
                }
            }
        }
    }
}