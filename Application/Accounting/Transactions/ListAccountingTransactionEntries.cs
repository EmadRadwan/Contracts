using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Shipments.Transactions;

public class ListAccountingTransactionEntries
{
    public class Query : IRequest<IQueryable<AccountingTransactionEntryRecord>>
    {
        public ODataQueryOptions<AccountingTransactionEntryRecord> Options { get; set; }
    }

    public class Handler : IRequestHandler<Query, IQueryable<AccountingTransactionEntryRecord>>
    {
        private readonly DataContext _context;
        private readonly ILogger<Handler> _logger;
        private readonly IMapper _mapper;
        private readonly IUserAccessor _userAccessor;

        public Handler(DataContext context, IMapper mapper, IUserAccessor userAccessor, ILogger<Handler> logger)
        {
            _mapper = mapper;
            _context = context;
            _userAccessor = userAccessor;
            _logger = logger;
        }

        public async Task<IQueryable<AccountingTransactionEntryRecord>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            var query = (from te in _context.AcctgTransEntries
                    join t in _context.AcctgTrans on te.AcctgTransId equals t.AcctgTransId into transJoin
                    from trans in transJoin.DefaultIfEmpty()
                    join a in _context.AcctgTransTypes on trans.AcctgTransTypeId equals a.AcctgTransTypeId into
                        transTypeJoin
                    from transType in transTypeJoin.DefaultIfEmpty()
                    join g in _context.GlAccountTypes on te.GlAccountTypeId equals g.GlAccountTypeId into glAccountJoin
                    from glAccount in glAccountJoin.DefaultIfEmpty()
                    join p in _context.Parties on te.PartyId equals p.PartyId into partyJoin
                    from party in partyJoin.DefaultIfEmpty()
                    select new AccountingTransactionEntryRecord
                    {
                        AcctgTransId = te.AcctgTransId,
                        AcctgTransEntrySeqId = te.AcctgTransEntrySeqId,
                        GlAccountTypeDescription = glAccount.Description,
                        AcctgTransactionTypeDescription = transType.Description,
                        Description = te.Description,
                        PartyId = te.PartyId,
                        PartyName = party != null ? party.Description : null,
                        ProductId = te.ProductId,
                        InvoiceId = trans.InvoiceId,
                        PaymentId = trans.PaymentId,
                        ShipmentId = trans.ShipmentId,
                        WorkEffortId = trans.WorkEffortId,
                        GlAccountTypeId = te.GlAccountTypeId,
                        GlAccountId = te.GlAccountId,
                        Amount = te.Amount,
                        DebitCreditFlag = te.DebitCreditFlag,
                        IsPosted = trans.IsPosted,
                        PostedDate = trans.PostedDate,
                        TransactionDate = trans.TransactionDate,
                        GlFiscalTypeId = trans.GlFiscalTypeId
                    }
                ).AsQueryable();

            /*var query = (from te in _context.AcctgTransEntries
                    join t in _context.AcctgTrans on te.AcctgTransId equals t.AcctgTransId into transJoin
                    from trans in transJoin.DefaultIfEmpty()
                    join a in _context.AcctgTransTypes on trans.AcctgTransTypeId equals a.AcctgTransTypeId into
                        transTypeJoin
                    from transType in transTypeJoin.DefaultIfEmpty()
                    join g in _context.GlAccountTypes on te.GlAccountTypeId equals g.GlAccountTypeId into glAccountJoin
                    from glAccount in glAccountJoin.DefaultIfEmpty()
                    join p in _context.Parties on te.PartyId equals p.PartyId into partyJoin
                    from party in partyJoin.DefaultIfEmpty()
                    select new
                    {
                        AccountingTransactionEntry = te,
                        AccountingTransaction = trans,
                        AccountingTransType = transType,
                        GlAccountType = glAccount,
                        Party = party
                    }).AsEnumerable()
                .GroupBy(x => x.AccountingTransaction?.AcctgTransId ?? Guid.Empty)
                .SelectMany(group => group.Select(item => new AccountingTransactionEntryRecord
                {
                    AcctgTransId = item.AccountingTransactionEntry.AcctgTransId,
                    AcctgTransEntrySeqId = item.AccountingTransactionEntry.AcctgTransEntrySeqId,
                    GlAccountTypeDescription = item.GlAccountType?.Description,
                    AcctgTransactionTypeDescription = item.AccountingTransType?.Description,
                    Description = item.AccountingTransactionEntry.Description,
                    PartyId = item.AccountingTransactionEntry.PartyId,
                    PartyName = item.Party?.Description,
                    ProductId = item.AccountingTransactionEntry.ProductId,
                    InvoiceId = item.AccountingTransaction?.InvoiceId,
                    PaymentId = item.AccountingTransaction?.PaymentId,
                    ShipmentId = item.AccountingTransaction?.ShipmentId,
                    WorkEffortId = item.AccountingTransaction?.WorkEffortId,
                    GlAccountTypeId = item.AccountingTransactionEntry.GlAccountTypeId,
                    GlAccountId = item.AccountingTransactionEntry.GlAccountId,
                    Amount = item.AccountingTransactionEntry.Amount,
                    DebitCreditFlag = item.AccountingTransactionEntry.DebitCreditFlag,
                    IsPosted = item.AccountingTransaction?.IsPosted,
                    PostedDate = item.AccountingTransaction?.PostedDate,
                    TransactionDate = item.AccountingTransaction?.TransactionDate,
                    GlFiscalTypeId = item.AccountingTransaction?.GlFiscalTypeId
                })).AsQueryable();*/


            return query;
        }
    }
}