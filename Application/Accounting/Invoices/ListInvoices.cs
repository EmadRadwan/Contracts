using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Logging;
using Persistence;
using Application.Shipments.Invoices;

namespace Application.Accounting.Invoices
{
    public class ListInvoices
    {
        public class Query : IRequest<IQueryable<InvoiceRecord>>
        {
            public ODataQueryOptions<InvoiceRecord> Options { get; set; }
            public string Language { get; set; } // REFACTOR: Added Language property to support "en" or "ar" for bilingual descriptions
        }

        public class Handler : IRequestHandler<Query, IQueryable<InvoiceRecord>>
        {
            private readonly DataContext _context;
            private readonly ILogger<Handler> _logger;

            public Handler(DataContext context, ILogger<Handler> logger)
            {
                _context = context;
                _logger = logger;
            }

            public async Task<IQueryable<InvoiceRecord>> Handle(Query request, CancellationToken cancellationToken)
            {
                var baseQuery = from inv in _context.Invoices
                    join invt in _context.InvoiceTypes on inv.InvoiceTypeId equals invt.InvoiceTypeId
                    join fromParty in _context.Parties on inv.PartyIdFrom equals fromParty.PartyId
                    join toParty in _context.Parties on inv.PartyId equals toParty.PartyId
                    join sts in _context.StatusItems on inv.StatusId equals sts.StatusId
                    join bil in _context.BillingAccounts on inv.BillingAccountId equals bil.BillingAccountId into billingGroup
                    from bil in billingGroup.DefaultIfEmpty()
                    join oib in _context.OrderItemBillings on inv.InvoiceId equals oib.InvoiceId into orderBillingGroup
                    from oib in orderBillingGroup.DefaultIfEmpty() // Left join for invoices without orders
                    join we in _context.WorkEfforts on new { oib.OrderId, WorkEffortTypeId = "PROJECT_CERTIFICATE" } 
                        equals new { OrderId = we.RelatedOrderId, we.WorkEffortTypeId } into workEffortGroup
                    from we in workEffortGroup.DefaultIfEmpty() // Left join for orders without project certificates
                    select new InvoiceRecord
                    {
                        InvoiceId = inv.InvoiceId,
                        InvoiceTypeDescription = request.Language == "ar" ? invt.DescriptionArabic : invt.Description,
                        InvoiceDate = inv.InvoiceDate,
                        StatusId = inv.StatusId,
                        InvoiceTypeId = inv.InvoiceTypeId,
                        StatusDescription = request.Language == "ar" ? sts.DescriptionArabic : sts.Description,
                        Description = inv.Description,
                        DueDate = inv.DueDate,
                        PaidDate = inv.PaidDate,
                        PartyId = new InvoicePartyDto
                        {
                            FromPartyId = inv.PartyId,
                            FromPartyName = toParty.Description
                        },
                        ToPartyName = toParty.Description,
                        PartyIdFrom = new InvoicePartyDto
                        {
                            FromPartyId = inv.PartyIdFrom,
                            FromPartyName = fromParty.Description
                        },
                        FromPartyName = fromParty.Description,
                        BillingAccountId = inv.BillingAccountId,
                        BillingAccountName = bil.Description,
                        Total = (from item in _context.InvoiceItems
                            where item.InvoiceId == inv.InvoiceId
                            select item.Quantity * item.Amount).Sum(),
                        OutstandingAmount = 0,
                        OrderId = oib != null ? oib.OrderId : null,
                        CertificateNumber = we != null ? we.CertificateNumber : null
                    };

                return baseQuery;
            }
        }
    }
}