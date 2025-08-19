using Application.Accounting.Services;
using Application.order.Orders;
using Application.Shipments;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Order.Orders;

public class ListPurchaseOrderItemsForReceive
{
    public class Query : IRequest<Result<ReceiveInventoryResponseDto>>
    {
        public ReceiveInventoryRequestDto ReceiveInventoryRequestDto { get; set; }
        public string Language { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<ReceiveInventoryResponseDto>>
    {
        private readonly DataContext _context;
        private readonly ILogger<Handler> _logger;
        private readonly IAcctgMiscService _acctgMiscService;
        private readonly IMediator _mediator;

        public Handler(DataContext context, ILogger<Handler> logger,
            IAcctgMiscService acctgMiscService, IMediator mediator)
        {
            _context = context;
            _logger = logger;
            _acctgMiscService = acctgMiscService;
            _mediator = mediator;
        }

        public async Task<Result<ReceiveInventoryResponseDto>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            try
            {
                // 1. Extract Parameters
                string? facilityId = request.ReceiveInventoryRequestDto.FacilityId;
                string purchaseOrderId = request.ReceiveInventoryRequestDto.PurchaseOrderId;
                string language = request.Language ?? "en";

                var orderShipments = await _context.OrderShipments
                    .Where(os => os.OrderId == purchaseOrderId)
                    .ToListAsync(cancellationToken);

                if (orderShipments.Count == 0)
                {
                    // Call QuickReceivePurchaseOrder.Handler
                    var quickReceiveQuery = new QuickReceivePurchaseOrder.Query
                    {
                        ReceiveInventoryRequestDto = request.ReceiveInventoryRequestDto
                    };

                    var quickReceiveResult = await _mediator.Send(quickReceiveQuery, cancellationToken);

                    if (!quickReceiveResult.IsSuccess)
                    {
                        _logger.LogWarning("QuickReceivePurchaseOrder failed: {Error}", quickReceiveResult.Error);
                        return Result<ReceiveInventoryResponseDto>.Failure(
                            "Failed to perform QuickReceivePurchaseOrder.");
                    }
                }

                // Initialize variables that depend on facilityId
                Facility? facility = null;
                PartyAcctgPreference? ownerAcctgPref = null;
                bool partialReceive = false; // Adjust based on your requirements


                if (!string.IsNullOrEmpty(facilityId))
                {
                    // 2. Validate Facility
                    facility = await _context.Facilities
                        .Include(f => f.OwnerParty) // Assuming navigation property
                        .FirstOrDefaultAsync(f => f.FacilityId == facilityId, cancellationToken);

                    if (facility == null)
                    {
                        _logger.LogWarning("Facility with ID {FacilityId} not found.", facilityId);
                        return Result<ReceiveInventoryResponseDto>.Failure($"Facility with ID {facilityId} not found.");
                    }

                    // 3. Fetch Owner Party and Accounting Preferences
                    if (facility.OwnerParty != null)
                    {
                        ownerAcctgPref = await _acctgMiscService.GetPartyAccountingPreferences(
                            facility.OwnerParty.PartyId);

                        if (ownerAcctgPref == null)
                        {
                            _logger.LogWarning("PartyAccountingPreference not found for OwnerParty ID {OwnerPartyId}.",
                                facility.OwnerParty.PartyId);
                            // Depending on requirements, decide to fail or proceed without accounting preferences
                        }
                    }
                    else
                    {
                        _logger.LogWarning("OwnerParty not related to Facility ID {FacilityId}.", facility.FacilityId);
                    }

                    // Set partialReceive based on your business logic
                    // For example, if it's the first time, partialReceive might be false
                }

                // 5. Validate Purchase Order
                var purchaseOrder = await _context.OrderHeaders
                    .FirstOrDefaultAsync(po => po.OrderId == purchaseOrderId, cancellationToken);

                if (purchaseOrder == null)
                {
                    _logger.LogWarning("Purchase Order with ID {PurchaseOrderId} not found.", purchaseOrderId);
                    return Result<ReceiveInventoryResponseDto>.Failure(
                        $"Purchase Order with ID {purchaseOrderId} not found.");
                }

                if (!string.Equals(purchaseOrder.OrderTypeId, "PURCHASE_ORDER", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("OrderTypeId for Purchase Order ID {PurchaseOrderId} is not 'PURCHASE_ORDER'.",
                        purchaseOrder.OrderId);
                    return Result<ReceiveInventoryResponseDto>.Failure(
                        $"OrderTypeId for Purchase Order ID {purchaseOrder.OrderId} is invalid.");
                }

                // 5. Fetch Order Items
                var orderItems = await _context.OrderItems
                    .Include(p => p.Product)
                    .GroupJoin(
                        _context.ProductFeatures,
                        oi => oi.ProductFeatureId,
                        pf => pf.ProductFeatureId,
                        (oi, pfGroup) => new { OrderItem = oi, ProductFeatures = pfGroup }
                    )
                    .SelectMany(
                        x => x.ProductFeatures.DefaultIfEmpty(),
                        (oi, pf) => new
                        {
                            OrderItem = oi.OrderItem,
                            Product = oi.OrderItem.Product,
                            ProductFeature = pf
                        }
                    )
                    .Where(x => x.OrderItem.OrderId == purchaseOrderId
                                && !new[] { "ITEM_CANCELLED", "ITEM_COMPLETED" }.Contains(x.OrderItem.StatusId))
                    .ToListAsync(cancellationToken);

                if (!orderItems.Any())
                {
                    _logger.LogWarning(
                        "No order items found for Purchase Order ID {PurchaseOrderId}.",
                        purchaseOrderId);
                    /*return Result<ReceiveInventoryResponseDto>.Failure(
                        "No order items found for the provided Purchase Order.");*/
                }

                // 7. Fetch SupplierPartyIds for all products related to the order
                var productIds = orderItems.Select(oi => oi.OrderItem.ProductId).Distinct().ToList();

                var supplierPartyIds = await _context.SupplierProducts
                    .Where(sp => productIds.Contains(sp.ProductId))
                    .Where(sp => sp.AvailableFromDate <= DateTime.UtcNow && sp.AvailableThruDate >= DateTime.UtcNow)
                    .OrderBy(sp => sp.PartyId)
                    .Select(sp => sp.PartyId)
                    .Distinct()
                    .ToListAsync(cancellationToken);


                // 7. Fetch Shipments (OrderShipment and ItemIssuance)
                List<ShipmentDto> shipments = new List<ShipmentDto>();

                if (!string.IsNullOrEmpty(facilityId))
                {
                    // Fetch OrderShipments
                    orderShipments = await _context.OrderShipments
                        .Where(os => os.OrderId == purchaseOrderId)
                        .ToListAsync(cancellationToken);

                    foreach (var orderShipment in orderShipments)
                    {
                        var shipment = await _context.Shipments
                            .FirstOrDefaultAsync(s => s.ShipmentId == orderShipment.ShipmentId, cancellationToken);

                        if (shipment != null &&
                            !"PURCH_SHIP_RECEIVED".Equals(shipment.StatusId, StringComparison.OrdinalIgnoreCase) &&
                            !"SHIPMENT_CANCELLED".Equals(shipment.StatusId, StringComparison.OrdinalIgnoreCase) &&
                            (string.IsNullOrEmpty(shipment.DestinationFacilityId) ||
                             shipment.DestinationFacilityId.Equals(facilityId, StringComparison.OrdinalIgnoreCase)))
                        {
                            shipments.Add(new ShipmentDto
                            {
                                ShipmentId = shipment.ShipmentId,
                                StatusId = shipment.StatusId,
                                DestinationFacilityId = shipment.DestinationFacilityId
                                // Map other properties as needed
                            });
                        }
                    }
                }


                // 8. Fetch ShipmentReceipts only if facilityId is provided
                Dictionary<string, double> shippedQuantities = new Dictionary<string, double>();

                foreach (var item in orderItems)
                {
                    double totalReceived = 0.0;

                    if (!string.IsNullOrEmpty(facilityId))
                    {
                        // Fetch ShipmentReceipts related to this OrderItem and Facility
                        var shipmentReceipts = await _context.ShipmentReceipts
                            .Where(sr => sr.OrderId == item.OrderItem.OrderId
                                         && sr.OrderItemSeqId == item.OrderItem.OrderItemSeqId
                                         && _context.InventoryItems.Any(ii => ii.InventoryItemId == sr.InventoryItemId
                                                                              && ii.FacilityId == facilityId))
                            .ToListAsync(cancellationToken);

                        foreach (var sr in shipmentReceipts)
                        {
                            totalReceived += (double)(sr.QuantityAccepted + sr.QuantityRejected);
                        }
                    }

                    shippedQuantities[item.OrderItem.OrderItemSeqId] = totalReceived;
                }

                // 9. Fetch Sales Order Item Associations
                Dictionary<string, SalesOrderItemAssocDto> salesOrderItems =
                    new Dictionary<string, SalesOrderItemAssocDto>();

                foreach (var item in orderItems)
                {
                    // Fetch related Sales Order Item Associations
                    var salesOrderItemAssocs = await _context.OrderItemAssocs
                        .Where(oia => oia.OrderItemAssocTypeId == "PURCHASE_ORDER"
                                      && oia.ToOrderId == item.OrderItem.OrderId
                                      && oia.ToOrderItemSeqId == item.OrderItem.OrderItemSeqId)
                        .ToListAsync(cancellationToken);

                    if (salesOrderItemAssocs.Any())
                    {
                        var assoc = salesOrderItemAssocs.First();
                        salesOrderItems[item.OrderItem.OrderItemSeqId] = new SalesOrderItemAssocDto
                        {
                            OrderId = assoc.OrderId,
                            OrderItemAssocTypeId = assoc.OrderItemAssocTypeId
                            // Map other properties as needed
                        };
                    }
                }

                // 10. Fetch Received Items only if facilityId is provided
                List<ShipmentReceiptAndItemDto> receivedItems = new List<ShipmentReceiptAndItemDto>();
                if (!string.IsNullOrEmpty(facilityId))
                {
                    var shipmentReceiptAndItems = await _context.ShipmentReceipts
                        .Join(
                            _context.InventoryItems,
                            sr => sr.InventoryItemId,
                            ii => ii.InventoryItemId,
                            (sr, ii) => new { ShipmentReceipt = sr, InventoryItem = ii }
                        )
                        .Join(
                            _context.Products,
                            sr_ii => sr_ii.ShipmentReceipt.ProductId,
                            p => p.ProductId,
                            (sr_ii, p) => new { sr_ii.ShipmentReceipt, sr_ii.InventoryItem, Product = p }
                        )
                        .Join(
                            _context.OrderItems,
                            sr_ii_p => new { sr_ii_p.ShipmentReceipt.OrderId, sr_ii_p.ShipmentReceipt.OrderItemSeqId },
                            oi => new { oi.OrderId, oi.OrderItemSeqId },
                            (sr_ii_p, oi) => new
                                { sr_ii_p.ShipmentReceipt, sr_ii_p.InventoryItem, sr_ii_p.Product, OrderItem = oi }
                        )
                        .Where(joined => joined.ShipmentReceipt.OrderId == purchaseOrderId
                                         && joined.InventoryItem.FacilityId == facilityId)
                        .Select(joined => new ShipmentReceiptAndItemDto
                        {
                            ReceiptId = joined.ShipmentReceipt.ReceiptId,
                            ShipmentId = joined.ShipmentReceipt.ShipmentId,
                            DatetimeReceived = joined.ShipmentReceipt.DatetimeReceived,
                            OrderId = joined.ShipmentReceipt.OrderId,
                            OrderItemSeqId = joined.ShipmentReceipt.OrderItemSeqId,
                            ProductId = joined.ShipmentReceipt.ProductId,
                            QuantityRejected = joined.ShipmentReceipt.QuantityRejected,
                            QuantityAccepted = joined.ShipmentReceipt.QuantityAccepted,
                            FacilityId = joined.InventoryItem.FacilityId,
                            LocationSeqId = joined.InventoryItem.LocationSeqId,
                            QuantityOnHandTotal = (double)joined.InventoryItem.QuantityOnHandTotal,
                            AvailableToPromiseTotal = (double)joined.InventoryItem.AvailableToPromiseTotal,
                            UnitCost = (decimal)joined.InventoryItem.UnitCost,
                            LotId = joined.InventoryItem.LotId,
                            ProductName = joined.Product.ProductName,
                            UnitPrice = (decimal)joined.OrderItem.UnitPrice // Added from OrderItems
                        })
                        .ToListAsync(cancellationToken);

                    receivedItems = shipmentReceiptAndItems;
                }

                // 11. Fetch Order Items and Map to DTOs
                List<OrderItemDto2> purchaseOrderItems = orderItems.Select(x =>
                {
                    var oi = x.OrderItem;
                    double alreadyReceived = 0;
                    shippedQuantities.TryGetValue(oi.OrderItemSeqId, out alreadyReceived);
                    double defaultQuantityToReceive = (double)oi.Quantity - alreadyReceived;
                    if (defaultQuantityToReceive < 0) defaultQuantityToReceive = 0;

                    string colorDescription = language == "ar" && x.ProductFeature?.DescriptionArabic != null
                        ? x.ProductFeature.DescriptionArabic
                        : x.ProductFeature?.Description ?? string.Empty;

                    string displayName = string.IsNullOrEmpty(colorDescription)
                        ? x.Product.ProductName
                        : $"{x.Product.ProductName} - {colorDescription}";

                    return new OrderItemDto2
                    {
                        OrderId = oi.OrderId,
                        OrderItemSeqId = oi.OrderItemSeqId,
                        ProductId = oi.ProductId,
                        ProductName = displayName,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        IncludeThisItem = false,
                        DefaultQuantityToReceive = (decimal?)defaultQuantityToReceive,
                        ProductFeatureId = x.ProductFeature?.ProductFeatureId
                    };
                }).ToList();


                // 12. Compile All Data into Response DTO
                var responseDto = new ReceiveInventoryResponseDto
                {
                    PartialReceive = partialReceive,
                    SupplierPartyIds = supplierPartyIds,
                    Shipments = shipments,
                    // Shipment remains null as shipmentId is not provided
                    Shipment = null,
                    ShippedQuantities = shippedQuantities,
                    PurchaseOrderItems = purchaseOrderItems,
                    // OrderCurrencyUnitPriceMap = orderCurrencyUnitPriceMap, // Uncomment if needed
                    ReceivedQuantities = shippedQuantities, // Assuming receivedQuantities is same as shippedQuantities
                    SalesOrderItems = salesOrderItems,
                    ReceivedItems = receivedItems,
                };


                // Return Success Result
                return Result<ReceiveInventoryResponseDto>.Success(responseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing ReceiveInventoryQuery.");
                return Result<ReceiveInventoryResponseDto>.Failure("An unexpected error occurred.");
            }
        }
    }
}