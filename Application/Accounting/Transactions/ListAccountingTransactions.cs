using API.Controllers.Accounting.Transactions;
using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Shipments.Transactions;

public class ListAccountingTransactions
{
    public class Query : IRequest<IQueryable<AccountingTransactionRecord>>
    {
        public ODataQueryOptions<AccountingTransactionRecord> Options { get; set; }
    }

    public class Handler : IRequestHandler<Query, IQueryable<AccountingTransactionRecord>>
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

        public async Task<IQueryable<AccountingTransactionRecord>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            var query = (from transaction in _context.AcctgTrans
                    join transactionType in _context.AcctgTransTypes on transaction.AcctgTransTypeId equals
                        transactionType.AcctgTransTypeId
                    select new AccountingTransactionRecord
                    {
                        AcctgTransId = transaction.AcctgTransId,
                        AcctgTransTypeId = transaction.AcctgTransTypeId,
                        AcctgTransTypeDescription = transactionType.Description,
                        PartyId = transaction.PartyId,
                        PaymentId = transaction.PaymentId,
                        TransactionDate = transaction.TransactionDate,
                        IsPosted = transaction.IsPosted,
                        PostedDate = transaction.PostedDate,
                        Description = transaction.Description,
                        InvoiceId = transaction.InvoiceId,
                        WorkEffortId = transaction.WorkEffortId,
                        ShipmentId = transaction.ShipmentId
                    }
                ).AsQueryable();


            return query;
        }
    }
}