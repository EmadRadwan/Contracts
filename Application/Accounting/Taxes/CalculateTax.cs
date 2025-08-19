using Application.Accounting.Services;
using Application.Order.Orders;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Domain;
using System.Text.Json;
using Application.Catalog.Products;
using Application.Catalog.ProductStores;

namespace Application.Accounting.Taxes;

public class CalculateTax
{
    public class Command : IRequest<Result<OrderAdjustmentDto2[]>>
    {
        public List<OrderItemDto2> OrderItems { get; set; }
        public List<OrderAdjustmentDto2> OrderAdjustments { get; set; }
    }

    public class Handler : IRequestHandler<Command, Result<OrderAdjustmentDto2[]>>
    {
        private readonly ILogger<CalculateTax> _logger;
        private readonly ILogger<TaxAuthorityServices> _taxServiceLogger;
        private readonly DataContext _dbContext;
        private readonly IProductService _productService;
        private readonly IProductStoreService _productStoreService;

        public Handler(
            ILogger<CalculateTax> logger,
            ILogger<TaxAuthorityServices> taxServiceLogger,
            DataContext dbContext,
            IProductService productService,
            IProductStoreService productStoreService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _taxServiceLogger = taxServiceLogger ?? throw new ArgumentNullException(nameof(taxServiceLogger));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _productStoreService = productStoreService ?? throw new ArgumentNullException(nameof(productStoreService));
        }

        public async Task<Result<OrderAdjustmentDto2[]>> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                // Instantiate TaxAuthorityServices
                var taxService = new TaxAuthorityServices(_dbContext, _taxServiceLogger, _productService);

                var orderItems = request.OrderItems;
                var orderId = request.OrderItems[0].OrderId;
                var partyId = "007cb303-495c-4bdb-b532-9d9f235e69c8"; // Default for debugging
                var productStoreId = await _productStoreService.GetProductStoreForLoggedInUser();

                // Get PostalAddress for the partyId
                var partyPostalAddress = await (from pcm in _dbContext.PartyContactMeches
                        join cm in _dbContext.ContactMeches on pcm.ContactMechId equals cm.ContactMechId
                        join pa in _dbContext.PostalAddresses on cm.ContactMechId equals pa.ContactMechId
                        where pcm.PartyId == partyId
                              && cm.ContactMechTypeId == "POSTAL_ADDRESS"
                              && pcm.ThruDate == null
                        select pa)
                    .SingleOrDefaultAsync(cancellationToken);

                if (partyPostalAddress == null)
                {
                    _logger.LogError("No postal address found for partyId: {PartyId}", partyId);
                    return Result<OrderAdjustmentDto2[]>.Failure("No postal address found for party");
                }

                // Validate and preprocess orderItems
                foreach (var item in orderItems)
                {
                    if (string.IsNullOrEmpty(item.OrderId)) item.OrderId = orderId;
                    if (string.IsNullOrEmpty(item.OrderItemSeqId)) item.OrderItemSeqId = Guid.NewGuid().ToString();
                    if (item.SubTotal == 0) item.SubTotal = item.Quantity * item.UnitPrice;
                    item.ProductTypeId ??= "DEFAULT_PRODUCT";
                }

                // REFACTORED: Calculate total order-level adjustments (promotions and non-promotions) to handle discounts, aligning with OFBiz's taxable base reduction
                var totalSubtotal = orderItems.Sum(item => (decimal)item.SubTotal);
                if (totalSubtotal == 0) totalSubtotal = 1; // Avoid division by zero
                var orderLevelAdjustments = request.OrderAdjustments
                    ?.Where(adj => string.IsNullOrEmpty(adj.OrderItemSeqId))
                    .Sum(adj => adj.Amount) ?? 0m;


                // Prepare inputs for RateProductTaxCalc
                var itemProductList = new List<Product>();
                var itemAmountList = new List<decimal>();
                var itemPriceList = new List<decimal>();
                var itemQuantityList = new List<decimal>();
                var itemShippingList = new List<decimal>();

                // Fetch Product entities and map to input lists
                foreach (var item in orderItems)
                {
                    var product = await _dbContext.Products
                        .AsNoTracking()
                        .FirstOrDefaultAsync(p => p.ProductId == item.ProductId, cancellationToken);

                    if (product == null)
                    {
                        _logger.LogError("Product not found for ProductId: {ProductId}", item.ProductId);
                        return Result<OrderAdjustmentDto2[]>.Failure(
                            $"Product not found for ProductId: {item.ProductId}");
                    }

                    // Calculate item-specific promotions
                    var itemPromotions = request.OrderAdjustments
                        ?.Where(adj => adj.OrderAdjustmentTypeId == "PROMOTION_ADJUSTMENT" &&
                                       adj.OrderItemSeqId == item.OrderItemSeqId)
                        .Sum(adj => adj.Amount) ?? 0m;

                    // Special Remark: Calculate item-specific discounts (non-promotions)
                    var itemDiscounts = request.OrderAdjustments
                        ?.Where(adj => adj.OrderAdjustmentTypeId != "PROMOTION_ADJUSTMENT" &&
                                       adj.OrderAdjustmentTypeId != "VAT_TAX" &&
                                       adj.OrderItemSeqId == item.OrderItemSeqId)
                        .Sum(adj => adj.Amount) ?? 0m;

                    // Distribute order-level adjustments proportionally
                    var orderAdjustmentShare = totalSubtotal != 0
                        ? (decimal)item.SubTotal / totalSubtotal * orderLevelAdjustments
                        : 0m;

                    // Special Remark: Include item-level discounts in adjusted amount
                    var adjustedAmount = (decimal)item.SubTotal + itemPromotions + itemDiscounts + orderAdjustmentShare;

                    // Skip items with zero or negative net amount (e.g., fully discounted promotional items)
                    if (adjustedAmount <= 0)
                    {
                        _logger.LogDebug(
                            "Skipping tax calculation for item {OrderItemSeqId} with net amount {AdjustedAmount}",
                            item.OrderItemSeqId, adjustedAmount);
                        continue;
                    }

                    itemProductList.Add(product);
                    itemAmountList.Add(adjustedAmount);
                    itemPriceList.Add((decimal)item.UnitPrice);
                    itemQuantityList.Add((decimal)item.Quantity);
                    itemShippingList.Add(0); // No shipping for draft orders
                }

                // Log fetched products and amounts
                _logger.LogDebug("Fetched Products: {ProductsJson}",
                    JsonSerializer.Serialize(
                        itemProductList.Select(p => new { p.ProductId, p.ProductName }),
                        new JsonSerializerOptions { WriteIndented = true }));
                _logger.LogDebug("Item Amounts: {Amounts}", string.Join(", ", itemAmountList));

                // Call RateProductTaxCalc
                var taxResult = await taxService.RateProductTaxCalc(
                    productStoreId: productStoreId.ProductStoreId,
                    facilityId: null,
                    payToPartyId: null,
                    billToPartyId: partyId,
                    itemProductList: itemProductList,
                    itemAmountList: itemAmountList,
                    itemPriceList: itemPriceList,
                    itemQuantityList: itemQuantityList,
                    itemShippingList: itemShippingList,
                    orderShippingAmount: null,
                    orderPromotionsAmount: null,
                    shippingAddress: partyPostalAddress
                );

                // Log TaxServiceResult
                if (taxResult.Success)
                {
                    var resultData = taxResult.Data as dynamic;
                    var orderAdjustments = resultData?.orderAdjustments as List<OrderAdjustment>;
                    var itemAdjustments = resultData?.itemAdjustments as List<List<OrderAdjustment>>;

                    _logger.LogDebug(
                        "TaxServiceResult Success: OrderAdjustments: {OrderAdjustmentsCount}, ItemAdjustments: {ItemAdjustmentsCount}",
                        orderAdjustments?.Count ?? 0, itemAdjustments?.Count ?? 0);

                    if (orderAdjustments?.Any() == true)
                    {
                        _logger.LogDebug("Order Adjustments: {AdjustmentsJson}",
                            JsonSerializer.Serialize(orderAdjustments,
                                new JsonSerializerOptions { WriteIndented = true }));
                    }

                    if (itemAdjustments?.Any() == true)
                    {
                        _logger.LogDebug("Item Adjustments: {AdjustmentsJson}",
                            JsonSerializer.Serialize(itemAdjustments,
                                new JsonSerializerOptions { WriteIndented = true }));
                    }
                }
                else
                {
                    _logger.LogError("TaxServiceResult Failed: {ErrorMessage}", taxResult.ErrorMessage);
                    return Result<OrderAdjustmentDto2[]>.Failure(taxResult.ErrorMessage);
                }

                // Convert TaxServiceResult to OrderAdjustmentDto2[]
                var resultDataDynamic = taxResult.Data as dynamic;
                var orderAdjustmentsList = (resultDataDynamic?.orderAdjustments as List<OrderAdjustment>) ??
                                           new List<OrderAdjustment>();
                var itemAdjustmentsList = (resultDataDynamic?.itemAdjustments as List<List<OrderAdjustment>>) ??
                                          new List<List<OrderAdjustment>>();

                // Flatten itemAdjustments for OrderAdjustmentDto2
                var allAdjustments = orderAdjustmentsList
                    .Concat(itemAdjustmentsList.SelectMany(x => x))
                    .Select((adj, index) => new OrderAdjustmentDto2
                    {
                        OrderAdjustmentId = Guid.NewGuid().ToString(),
                        OrderAdjustmentTypeId = adj.OrderAdjustmentTypeId,
                        // REFACTORED: Added OrderAdjustmentTypeDescription by fetching from OrderAdjustmentType
                        OrderAdjustmentTypeDescription = _dbContext.OrderAdjustmentTypes
                            .Where(t => t.OrderAdjustmentTypeId == adj.OrderAdjustmentTypeId)
                            .Select(t => t.Description)
                            .FirstOrDefault() ?? adj.OrderAdjustmentTypeId,
                        Amount = adj.Amount ?? 0,
                        OrderItemSeqId = itemAdjustmentsList.Any() &&
                                         itemAdjustmentsList.Any(inner => inner.Contains(adj))
                            ? orderItems[itemAdjustmentsList.FindIndex(inner => inner.Contains(adj))].OrderItemSeqId
                            : null,
                        OrderId = orderId,
                        IsManual = "N",
                        IsAdjustmentDeleted = false,
                        // REFACTORED: Retained additional fields for VAT/exemption support
                        ProductPromoId = adj.ProductPromoId,
                        ProductPromoRuleId = adj.ProductPromoRuleId,
                        ProductPromoActionSeqId = adj.ProductPromoActionSeqId,
                        OverrideGlAccountId = adj.OverrideGlAccountId,
                        Comments = adj.Comments,
                        Description = adj.Description,
                        LastModifiedDate = adj.LastModifiedDate,
                        CreatedDate = adj.CreatedDate,
                        TaxAuthorityRateSeqId = adj.TaxAuthorityRateSeqId,
                        TaxAuthGeoId = adj.TaxAuthGeoId,
                        TaxAuthPartyId = adj.TaxAuthPartyId,
                        SourcePercentage = adj.SourcePercentage,
                        CorrespondingProductId = adj.CorrespondingProductId,
                    })
                    .ToArray();

                // Update TotalItemTaxAdjustments in orderItems
                foreach (var item in orderItems)
                {
                    item.TotalItemTaxAdjustments = allAdjustments
                        .Where(adj => adj.OrderItemSeqId == item.OrderItemSeqId)
                        .Sum(adj => adj.Amount);
                }

                // Log final adjustments
                _logger.LogDebug("Final OrderAdjustmentDto2: {AdjustmentsJson}",
                    JsonSerializer.Serialize(allAdjustments, new JsonSerializerOptions { WriteIndented = true }));

                return Result<OrderAdjustmentDto2[]>.Success(allAdjustments);
            }
            catch (Exception ex)
            {
                return Result<OrderAdjustmentDto2[]>.Failure(
                    $"Error calculating tax adjustments for order: {ex.Message}");
            }
        }
    }
}