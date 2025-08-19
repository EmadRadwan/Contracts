using API.Controllers.Accounting.Transactions;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Shipments.Transactions;

public class GetPaymentTransactionEntries
{
    public class Query : IRequest<Result<List<AcctgTransEntryDto>>>
    {
        public string PaymentId { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<List<AcctgTransEntryDto>>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<Result<List<AcctgTransEntryDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            try
            {
                var invoiceTransactionEntries = await _context.AcctgTransEntries
                    .Join(_context.AcctgTrans,
                        acctgTransEntry => acctgTransEntry.AcctgTransId,
                        acctgTrans => acctgTrans.AcctgTransId,
                        (acctgTransEntry, acctgTrans) => new
                            { AcctgTransEntry = acctgTransEntry, AcctgTrans = acctgTrans })
                    .GroupJoin(_context.Products,
                        joinedData => joinedData.AcctgTransEntry.ProductId,
                        product => product.ProductId,
                        (joinedData, products) => new { joinedData, Products = products.DefaultIfEmpty() })
                    .Where(c => c.joinedData.AcctgTrans.PaymentId == request.PaymentId)
                    .Select(c => new AcctgTransEntryDto
                    {
                        AcctgTransId = c.joinedData.AcctgTransEntry.AcctgTransId,
                        AcctgTransEntrySeqId = c.joinedData.AcctgTransEntry.AcctgTransEntrySeqId,
                        AcctgTransTypeDescription = c.joinedData.AcctgTrans.AcctgTransType.Description,
                        Description = c.joinedData.AcctgTransEntry.Description,
                        VoucherRef = c.joinedData.AcctgTransEntry.VoucherRef,
                        PartyId = c.joinedData.AcctgTransEntry.PartyId,
                        RoleTypeId = c.joinedData.AcctgTransEntry.RoleTypeId,
                        TheirPartyId = c.joinedData.AcctgTransEntry.TheirPartyId,
                        ProductId = c.joinedData.AcctgTransEntry.ProductId,
                        TheirProductId = c.joinedData.AcctgTransEntry.TheirProductId,
                        InventoryItemId = c.joinedData.AcctgTransEntry.InventoryItemId,
                        GlAccountTypeId = c.joinedData.AcctgTransEntry.GlAccountTypeId,
                        GlAccountTypeDescription = c.joinedData.AcctgTransEntry.GlAccount.AccountName,
                        GlAccountClassDescription = c.joinedData.AcctgTransEntry.GlAccount.GlAccountClass.Description,
                        GlAccountId = c.joinedData.AcctgTransEntry.GlAccountId,
                        OrganizationPartyId = c.joinedData.AcctgTransEntry.OrganizationPartyId,
                        Amount = c.joinedData.AcctgTransEntry.Amount,
                        CurrencyUomId = c.joinedData.AcctgTransEntry.CurrencyUomId,
                        OrigAmount = c.joinedData.AcctgTransEntry.OrigAmount,
                        OrigCurrencyUomId = c.joinedData.AcctgTransEntry.OrigCurrencyUomId,
                        DebitCreditFlag = c.joinedData.AcctgTransEntry.DebitCreditFlag,
                        DueDate = c.joinedData.AcctgTransEntry.DueDate,
                        GroupId = c.joinedData.AcctgTransEntry.GroupId,
                        TaxId = c.joinedData.AcctgTransEntry.TaxId,
                        ReconcileStatusId = c.joinedData.AcctgTransEntry.ReconcileStatusId,
                        SettlementTermId = c.joinedData.AcctgTransEntry.SettlementTermId,
                        IsSummary = c.joinedData.AcctgTransEntry.IsSummary,
                        IsPosted = c.joinedData.AcctgTrans.IsPosted,
                        ProductName = c.Products.FirstOrDefault() != null ? c.Products.First().ProductName : null,
                        GlFiscalTypeId = c.joinedData.AcctgTrans.GlFiscalTypeId,
                        TransactionDate = c.joinedData.AcctgTrans.TransactionDate,
                        PostedDate = c.joinedData.AcctgTrans.PostedDate
                    })
                    .ToListAsync(cancellationToken);

                return Result<List<AcctgTransEntryDto>>.Success(invoiceTransactionEntries);
            }
            catch (Exception ex)
            {
                // Handle exception and return an error result
                return Result<List<AcctgTransEntryDto>>.Failure(ex.Message);
            }
        }
    }
}