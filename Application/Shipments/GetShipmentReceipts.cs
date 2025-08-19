using Application.Catalog.ProductStores;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Shipments;

public class GetShipmentReceipts
{
    public class Query : IRequest<Result<List<ShipmentReceiptDTo>>>
    {
        public string OrderId { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<List<ShipmentReceiptDTo>>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IProductStoreService _productStoreService;

        public Handler(DataContext context, IMapper mapper, IProductStoreService productStoreService)
        {
            _context = context;
            _mapper = mapper;
            _productStoreService = productStoreService;
        }


        public async Task<Result<List<ShipmentReceiptDTo>>> Handle(Query request, CancellationToken cancellationToken)
        {
            // get a list of shipment receipts for a certain purchase order 
            // and populate the ShipmentReceiptDTo object and also the ProductName property
            var shipmentReceipts = await _context.ShipmentReceipts
                .Where(x => x.OrderId == request.OrderId)
                .Join(
                    _context.Products, // Table to join with
                    receipt => receipt.ProductId, // Key selector in ShipmentReceipts
                    product => product.ProductId, // Key selector in Products   
                    (receipt, product) => new ShipmentReceiptDTo
                    {
                        ReceiptId = receipt.ReceiptId,
                        OrderId = receipt.OrderId,
                        OrderItemSeqId = receipt.OrderItemSeqId,
                        ProductId = receipt.ProductId,
                        ProductName = product.ProductName,
                        QuantityAccepted = receipt.QuantityAccepted,
                        QuantityRejected = receipt.QuantityRejected,
                        DatetimeReceived = receipt.DatetimeReceived
                    })
                .ToListAsync(cancellationToken);

            return await Task.FromResult(Result<List<ShipmentReceiptDTo>>.Success(shipmentReceipts));
        }
    }
}