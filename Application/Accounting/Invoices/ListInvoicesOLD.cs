using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Shipments.Invoices;

public class ListInvoicesOLD
{
    public class Query : IRequest<IQueryable<InvoiceRecord>>
    {
        public ODataQueryOptions<InvoiceRecord> Options { get; set; }
    }

    public class Handler : IRequestHandler<Query, IQueryable<InvoiceRecord>>
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

        public async Task<IQueryable<InvoiceRecord>> Handle(Query request, CancellationToken cancellationToken)
        {
            var invoicesQuery = from inv in _context.Invoices
                join pty in _context.Parties on inv.PartyIdFrom equals pty.PartyId
                join ptyto in _context.Parties on inv.PartyId equals ptyto.PartyId
                join invt in _context.InvoiceTypes on inv.InvoiceTypeId equals invt.InvoiceTypeId
                join sts in _context.StatusItems on inv.StatusId equals sts.StatusId
                join uom in _context.Uoms on inv.CurrencyUomId equals uom.UomId
                join item in _context.InvoiceItems on inv.InvoiceId equals item.InvoiceId into itemsGroup
                select new
                {
                    Invoice = inv,
                    PartyFrom = pty,
                    PartyTo = ptyto,
                    InvoiceType = invt,
                    Status = sts,
                    CurrencyUom = uom.UomId,
                    ItemsGroup = itemsGroup
                };

            var paymentAppsQuery = from inv in _context.Invoices
                join paymentApp in _context.PaymentApplications on inv.InvoiceId equals paymentApp.InvoiceId into
                    paymentAppsGroup
                select new
                {
                    inv.InvoiceId,
                    PaymentAppsGroup = paymentAppsGroup
                };

            var joinedQuery = from inv in invoicesQuery
                join paymentApp in paymentAppsQuery on inv.Invoice.InvoiceId equals paymentApp.InvoiceId into
                    tempPaymentApps
                from paymentApp in tempPaymentApps.DefaultIfEmpty()
                select new InvoiceRecord
                {
                    InvoiceId = inv.Invoice.InvoiceId,
                    InvoiceTypeId = inv.Invoice.InvoiceTypeId,
                    InvoiceTypeDescription = inv.InvoiceType.Description,
                    CurrencyUomId = inv.Invoice.CurrencyUomId,
                    PartyId = new InvoicePartyDto
                    {
                        FromPartyId = inv.Invoice.PartyId,
                        FromPartyName = inv.PartyTo.Description
                    },
                    ToPartyName = inv.PartyTo.Description,
                    PartyIdFrom = new InvoicePartyDto
                    {
                        FromPartyId = inv.Invoice.PartyIdFrom,
                        FromPartyName = inv.PartyFrom.Description
                    },
                    FromPartyName = inv.PartyFrom.Description,
                    StatusDescription = inv.Status.Description,
                    InvoiceDate = inv.Invoice.InvoiceDate,
                    DueDate = inv.Invoice.DueDate,
                    PaidDate = inv.Invoice.PaidDate,
                    Total = inv.ItemsGroup.Sum(item => item.Amount),
                    OutstandingAmount = inv.ItemsGroup.Sum(item => item.Amount) - (paymentApp != null
                        ? paymentApp.PaymentAppsGroup.Sum(pa => pa.AmountApplied)
                        : 0)
                };

            return joinedQuery.AsQueryable();
        }
    }
}