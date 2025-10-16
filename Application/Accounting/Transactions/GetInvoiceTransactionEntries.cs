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

            public async Task<Result<List<AcctgTransEntryDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {
                    // REFACTOR: Normalize language input to lowercase and default to "en"
                    // Ensures consistent language handling and prevents null reference issues
                    var language = request.Language?.ToLower() ?? "en";

                    var query = _context.AcctgTransEntries
                        .Join(_context.AcctgTrans,
                            acctgTransEntry => acctgTransEntry.AcctgTransId,
                            acctgTrans => acctgTrans.AcctgTransId,
                            (acctgTransEntry, acctgTrans) => new { AcctgTransEntry = acctgTransEntry, AcctgTrans = acctgTrans })
                        .GroupJoin(_context.Products,
                            joinedData => joinedData.AcctgTransEntry.ProductId,
                            product => product.ProductId,
                            (joinedData, products) => new { joinedData, Products = products.DefaultIfEmpty() })
                        .Where(c =>
                            c.joinedData.AcctgTrans.InvoiceId == request.InvoiceId &&
                            c.joinedData.AcctgTrans.AcctgTransTypeId == request.AcctgTransTypeId)
                        .Select(c => new AcctgTransEntryDto
                        {
                            AcctgTransId = c.joinedData.AcctgTransEntry.AcctgTransId,
                            AcctgTransEntrySeqId = c.joinedData.AcctgTransEntry.AcctgTransEntrySeqId,
                            AcctgTransTypeDescription = c.joinedData.AcctgTrans.AcctgTransType.Description,
                            Description = c.joinedData.AcctgTransEntry.Description,
                            PartyId = c.joinedData.AcctgTransEntry.PartyId,
                            ProductId = c.joinedData.AcctgTransEntry.ProductId,
                            GlAccountTypeId = c.joinedData.AcctgTransEntry.GlAccountTypeId,
                            // REFACTOR: Add Arabic support for GlAccountTypeDescription
                            // Selects AccountNameArabic for "ar" language, falls back to AccountName if null or language is "en"
                            GlAccountTypeDescription = c.joinedData.AcctgTransEntry.GlAccount != null
                                ? (language == "ar"
                                    ? c.joinedData.AcctgTransEntry.GlAccount.AccountNameArabic ?? c.joinedData.AcctgTransEntry.GlAccount.AccountName
                                    : c.joinedData.AcctgTransEntry.GlAccount.AccountName)
                                : "N/A",
                            GlAccountId = c.joinedData.AcctgTransEntry.GlAccountId,
                            OrganizationPartyId = c.joinedData.AcctgTransEntry.OrganizationPartyId,
                            Amount = c.joinedData.AcctgTransEntry.Amount,
                            CurrencyUomId = c.joinedData.AcctgTransEntry.CurrencyUomId,
                            OrigAmount = c.joinedData.AcctgTransEntry.OrigAmount,
                            OrigCurrencyUomId = c.joinedData.AcctgTransEntry.OrigCurrencyUomId,
                            DebitCreditFlag = c.joinedData.AcctgTransEntry.DebitCreditFlag,
                            DueDate = c.joinedData.AcctgTransEntry.DueDate,
                            IsPosted = c.joinedData.AcctgTrans.IsPosted,
                            TransactionDate = c.joinedData.AcctgTrans.TransactionDate,
                            PostedDate = c.joinedData.AcctgTrans.PostedDate,
                            ProductName = c.Products.FirstOrDefault() != null ? c.Products.First().ProductName : null
                        });

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