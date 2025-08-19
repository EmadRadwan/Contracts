using AutoMapper;
using MediatR;
using Persistence;
using Application.Catalog.Products;
using Microsoft.EntityFrameworkCore;

namespace Application.Shipments.Invoices;

public class ListInvoiceItems
{
    public class Query : IRequest<Result<List<InvoiceItemDto2>>>
    {
        public string InvoiceId { get; set; }
        public string Language { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<List<InvoiceItemDto2>>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<Result<List<InvoiceItemDto2>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var language = request.Language;

            var invoiceItems = await (from itm in _context.InvoiceItems
                join prd in _context.Products on itm.ProductId equals prd.ProductId into prdJoin
                from prd in prdJoin.DefaultIfEmpty()
                join invit in _context.InvoiceItemTypes on itm.InvoiceItemTypeId equals invit.InvoiceItemTypeId
                where itm.InvoiceId == request.InvoiceId
                select new InvoiceItemDto2
                {
                    InvoiceId = itm.InvoiceId,
                    InvoiceItemSeqId = itm.InvoiceItemSeqId,
                    ProductId = itm.ProductId,
                    Description = itm.Description,
                    InvoiceItemTypeId = itm.InvoiceItemTypeId,
                    InvoiceItemTypeDescription = language == "ar" ? invit.DescriptionArabic : invit.Description,
                    ProductName = prd.ProductName,
                    Amount = itm.Amount,
                    Quantity = itm.Quantity
                }).ToListAsync(cancellationToken: cancellationToken);

            var result = new List<InvoiceItemDto2>();

            foreach (var invoiceItem in invoiceItems)
            {
                // Only query for product details if a ProductId is provided.
                if (invoiceItem.ProductId != null)
                {
                    var invoiceItemProduct = await (from prd in _context.Products
                        join prc in _context.ProductPrices on prd.ProductId equals prc.ProductId
                        join inv in _context.InventoryItems on prd.ProductId equals inv.ProductId
                        join fac in _context.Facilities on inv.FacilityId equals fac.FacilityId
                        where prd.ProductId == invoiceItem.ProductId
                        select new ProductLovDto
                        {
                            ProductId = prd.ProductId,
                            ProductName = prd.ProductName,
                            FacilityName = fac.FacilityName,
                            InventoryItem = inv.InventoryItemId,
                            QuantityOnHandTotal = inv.QuantityOnHandTotal,
                            AvailableToPromiseTotal = inv.AvailableToPromiseTotal,
                            Price = prc.Price
                        }).FirstOrDefaultAsync();

                    invoiceItem.InvoiceItemProduct = invoiceItemProduct;
                }

                result.Add(invoiceItem);
            }

            return Result<List<InvoiceItemDto2>>.Success(result);
        }
    }
}
