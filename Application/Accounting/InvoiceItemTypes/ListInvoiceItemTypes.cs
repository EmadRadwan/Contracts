using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Shipments.InvoiceItemTypes;

public class ListInvoiceItemTypes
{
    public class Query : IRequest<IQueryable<InvoiceItemTypeRecord>>
    {
        public ODataQueryOptions<InvoiceItemTypeRecord> Options { get; set; }
    }

    public class Handler : IRequestHandler<Query, IQueryable<InvoiceItemTypeRecord>>
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

        public async Task<IQueryable<InvoiceItemTypeRecord>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            var query = (from invoiceItemType in _context.InvoiceItemTypes
                select new InvoiceItemTypeRecord
                {
                    InvoiceItemTypeId = invoiceItemType.InvoiceItemTypeId,
                    ParentTypeId = invoiceItemType.ParentTypeId,
                    HasTable = invoiceItemType.HasTable,
                    Description = invoiceItemType.Description,
                    DefaultGlAccountId = invoiceItemType.DefaultGlAccountId + " - " + invoiceItemType.DefaultGlAccount.AccountName,
                }).AsQueryable();

            return query;
        }
    }
}