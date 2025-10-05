using Application.Interfaces;
using Application.Shipments.Transactions;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Accounting.Transactions;

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

        public async Task<IQueryable<AccountingTransactionEntryRecord>> Handle(Query request, CancellationToken cancellationToken)
        {
            // REFACTOR: Replaced join with GlAccountTypes to join with GlAccounts to retrieve the account name.
            // This change aligns with the requirement to include GlAccount name instead of GlAccountType description.
            // The join uses GlAccountId from AcctgTransEntries to match with GlAccounts, and the AccountName is mapped to the record.
            var query = (from te in _context.AcctgTransEntries
                         join t in _context.AcctgTrans on te.AcctgTransId equals t.AcctgTransId into transJoin
                         from trans in transJoin.DefaultIfEmpty()
                         join a in _context.AcctgTransTypes on trans.AcctgTransTypeId equals a.AcctgTransTypeId into transTypeJoin
                         from transType in transTypeJoin.DefaultIfEmpty()
                         join g in _context.GlAccounts on te.GlAccountId equals g.GlAccountId into glAccountJoin
                         from glAccount in glAccountJoin.DefaultIfEmpty()
                         join p in _context.Parties on te.PartyId equals p.PartyId into partyJoin
                         from party in partyJoin.DefaultIfEmpty()
                         select new AccountingTransactionEntryRecord
                         {
                             AcctgTransId = te.AcctgTransId,
                             AcctgTransEntrySeqId = te.AcctgTransEntrySeqId,
                             // REFACTOR: Changed GlAccountTypeDescription to GlAccountName to reflect the new join with GlAccounts.
                             // This provides the account name instead of the account type description, as requested.
                             GlAccountName = glAccount != null ? glAccount.AccountName : null,
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
                         }).AsQueryable();

            return query;
        }
    }
}