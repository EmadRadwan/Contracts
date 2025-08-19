using Application.Catalog.Products;
using AutoMapper;
using MediatR;
using Persistence;

namespace Application.Shipments.Invoices;

public class ListInvoice
{
    public class Query : IRequest<Result<InvoiceDto2>>
    {
        public string InvoiceId { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<InvoiceDto2>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<Result<InvoiceDto2>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = from inv in _context.Invoices
                join invt in _context.InvoiceTypes on inv.InvoiceTypeId equals invt.InvoiceTypeId
                join sts in _context.StatusItems on inv.StatusId equals sts.StatusId
                join pty in _context.Parties on inv.PartyIdFrom equals pty.PartyId
                join ptyto in _context.Parties on inv.PartyId equals ptyto.PartyId
                where inv.InvoiceId == request.InvoiceId
                select new InvoiceDto2
                {
                    InvoiceId = inv.InvoiceId,
                    InvoiceTypeId = inv.InvoiceTypeId,
                    InvoiceTypeDescription = invt.Description,
                    PartyIdFrom = new InvoicePartyDto
                    {
                        FromPartyId = pty.PartyId,
                        FromPartyName = pty.Description
                    },
                    PartyId = new InvoicePartyDto
                    {
                        FromPartyId = ptyto.PartyId,
                        FromPartyName = ptyto.Description
                    },
                    InvoiceDate = inv.InvoiceDate,
                    StatusDescription = sts.Description,
                    AllowSubmit = false
                };


            var results = query.ToList();

            var invoiceItems = (from itm in _context.InvoiceItems
                join invit in _context.InvoiceItemTypes on itm.InvoiceItemTypeId equals invit.InvoiceItemTypeId
                join prd in _context.Products on itm.ProductId equals prd.ProductId
                    into facs
                from prd in facs.DefaultIfEmpty()
                join prc in _context.ProductPrices on prd.ProductId equals prc.ProductId
                join inv in _context.InventoryItems on prd.ProductId equals inv.ProductId
                join fac in _context.Facilities on inv.FacilityId equals fac.FacilityId
                where itm.InvoiceId == request.InvoiceId
                select new InvoiceItemDto
                {
                    InvoiceId = itm.InvoiceId,
                    InvoiceItemSeqId = itm.InvoiceItemSeqId,
                    ProductId = new ProductLovDto
                    {
                        ProductId = itm.ProductId,
                        ProductName = prd.ProductName,
                        FacilityName = fac.FacilityName,
                        InventoryItem = inv.InventoryItemId,
                        QuantityOnHandTotal = inv.QuantityOnHandTotal,
                        AvailableToPromiseTotal = inv.AvailableToPromiseTotal,
                        Price = prc.PriceWithTax
                    },
                    ProductName = prd.ProductName,
                    Quantity = itm.Quantity,
                    IsInvoiceItemDeleted = false,
                    InvoiceItemTypeId = itm.InvoiceItemTypeId,
                    InvoiceItemTypeName = invit.Description,
                    UnitPrice = prc.PriceWithTax,
                    OverrideGlAccountId = itm.OverrideGlAccountId,
                    InventoryItemId = itm.InventoryItemId,
                    Amount = itm.Amount,
                    Description = itm.Description
                }).ToList();

            var invoiceToReturn = new InvoiceDto2();

            if (results.Any())
            {
                invoiceToReturn = results[0];
                if (invoiceItems.Any()) invoiceToReturn.InvoiceItems = invoiceItems;
            }

            return Result<InvoiceDto2>.Success(invoiceToReturn);
        }
    }
}