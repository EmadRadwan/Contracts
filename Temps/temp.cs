public async Task<Result<List<OrderItemDto2>>> Handle(Query request, CancellationToken cancellationToken)
{
    if (string.IsNullOrEmpty(request.OrderId))
    {
        return Result<List<OrderItemDto2>>.Failure("OrderId cannot be null or empty.");
    }

    var language = request.Language ?? "en";

    var productStoreInventoryFacilityId = await _productStoreService.GetProductFacilityId();

    // REFACTOR: Added left join with Uoms table and included UomId and UomName in the OrderItemDto2 projection.
    // This fetches UOM data directly in the initial query, aligning with the requirement to place UomId and UomName
    // at the same level as ProductId and ProductName, reducing dependency on the nested OrderItemProduct.
    var orderItems = await (from itm in _context.OrderItems.AsNoTracking()
        join prd in _context.Products.AsNoTracking() on itm.ProductId equals prd.ProductId
        join uom in _context.Uoms.AsNoTracking() on prd.QuantityUomId equals uom.UomId into uomGroup
        from uom in uomGroup.DefaultIfEmpty()
        where itm.OrderId == request.OrderId
        let discountAdjustments = _context.OrderAdjustments
            .AsNoTracking()
            .Where(adjustment => adjustment.OrderId == itm.OrderId
                                 && adjustment.OrderItemSeqId == itm.OrderItemSeqId
                                 && adjustment.OrderAdjustmentTypeId == "DISCOUNT_ADJUSTMENT")
            .ToList()
        let totalDiscountAdjustments = discountAdjustments.Sum(adjustment => adjustment.Amount)
        select new OrderItemDto2
        {
            OrderId = itm.OrderId,
            OrderItemSeqId = itm.OrderItemSeqId,
            ProductId = itm.ProductId,
            ProductName = prd.ProductName,
            Quantity = itm.Quantity,
            UnitPrice = itm.UnitPrice,
            SubTotal = itm.Quantity * itm.UnitPrice,
            IsProductDeleted = false,
            FacilityId = productStoreInventoryFacilityId,
            ValidItem = true,
            TotalItemTaxAdjustments = _context.OrderAdjustments
                .AsNoTracking()
                .Where(adjustment => adjustment.OrderId == itm.OrderId
                                     && adjustment.OrderItemSeqId == itm.OrderItemSeqId
                                     && (adjustment.OrderAdjustmentTypeId == "SALES_TAX"
                                         || adjustment.OrderAdjustmentTypeId == "VAT_TAX"))
                .Sum(adjustment => adjustment.Amount),
            DiscountAndPromotionAdjustments = totalDiscountAdjustments,
            UomId = uom != null ? uom.UomId : null,
            UomName = uom != null ? (language == "ar" ? uom.DescriptionArabic : uom.Description) : null
        }).ToListAsync(cancellationToken);

    var result = new List<OrderItemDto2>();

    foreach (var orderItem in orderItems)
    {
        var shipmentReceipts = _context.ShipmentReceipts
            .AsNoTracking()
            .Where(x => x.OrderId == orderItem.OrderId && x.OrderItemSeqId == orderItem.OrderItemSeqId)
            .ToList();

        orderItem.QuantityAccepted = shipmentReceipts.Sum(x => x.QuantityAccepted) ?? 0;
        orderItem.QuantityRejected = shipmentReceipts.Sum(x => x.QuantityRejected) ?? 0;
        orderItem.IncludeThisItem = false;

        var orderItemProduct = await (from prd in _context.Products.AsNoTracking()
            join sp in _context.SupplierProducts.AsNoTracking() on prd.ProductId equals sp.ProductId into spGroup
            from sp in spGroup.DefaultIfEmpty()
            join uom in _context.Uoms.AsNoTracking() on prd.QuantityUomId equals uom.UomId into uomGroup
            from uom in uomGroup.DefaultIfEmpty()
            join iif in _context.InventoryItemFeatures.AsNoTracking() on prd.ProductId equals iif.ProductId into iifGroup
            from iif in iifGroup.DefaultIfEmpty()
            join pf in _context.ProductFeatures.AsNoTracking()
                    .Where(pf => pf.ProductFeatureTypeId == "COLOR") on iif != null
                    ? iif.ProductFeatureId
                    : null
                equals pf.ProductFeatureId into pfGroup
            from pf in pfGroup.DefaultIfEmpty()
            where prd.ProductId == orderItem.ProductId
            select new ProductLovDto
            {
                ProductId = prd.ProductId,
                ProductName = prd.ProductName,
                ColorDescription = pf != null ? (language == "ar" ? pf.DescriptionArabic : pf.Description) : null,
                LastPrice = sp != null ? sp.LastPrice : null,
                QuantityUom = uom != null ? uom.UomId : null,
                UomDescription = uom != null ? (language == "ar" ? uom.DescriptionArabic : uom.Description) : null
            }).FirstOrDefaultAsync(cancellationToken);

        if (orderItemProduct != null)
        {
            // REFACTOR: Updated ProductName assignment to maintain existing behavior.
            // Since UomId and UomName are now fetched in the initial query, no UOM-related assignments are needed here.
            orderItem.ProductName = orderItemProduct.ProductName + " " +
                                    (orderItemProduct.ColorDescription ?? string.Empty);
            orderItem.OrderItemProduct = orderItemProduct;
        }

        result.Add(orderItem);
    }

    return Result<List<OrderItemDto2>>.Success(result);
}