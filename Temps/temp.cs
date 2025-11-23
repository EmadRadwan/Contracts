if (certificate.CertificateCategory != "COMPANY_SUPPLY_SALE_CERTIFICATE")
{
    var orderItems = new List<OrderItemDto2>();
    var orderAdjustments = new List<OrderAdjustmentDto2>();
    var seq = 1;

    var fromPartyId = certificate.CertificateCategory switch
    {
        "SUPPLY_PROCUREMENT_CERTIFICATE" => certificate.PartyIdSupplier 
            ?? throw new InvalidOperationException("PartyIdSupplier is required"),
        "WORKMANSHIP_CONTRACTING_CERTIFICATE" => certificate.PartyIdContractor 
            ?? throw new InvalidOperationException("PartyIdContractor is required"),
        _ => throw new InvalidOperationException($"Unsupported category: {certificate.CertificateCategory}")
    };

    foreach (var item in certificate.CertificateItems!)
    {
        var mainSeqId = seq.ToString("D4");

        if (certificate.CertificateCategory == "WORKMANSHIP_CONTRACTING_CERTIFICATE")
        {
            // MAIN ITEM: Quantity = 1, UnitPrice = net (exact payable amount)
            orderItems.Add(new OrderItemDto2
            {
                OrderItemSeqId = mainSeqId,
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Quantity = 1m,
                UnitPrice = item.Net,                    // ← EXACT net amount
                SubTotal = item.Net,
                UomId = item.UomId,
                FacilityId = certificate.FacilityId,
                ItemDescription = item.Description,
                OrderItemTypeId = "PROJECT_CERTIFICATE_ITEM",
                StatusId = "ITEM_CREATED",
                CreatedStamp = stamp,
                LastUpdatedStamp = stamp
            });

            // INSURANCE AS NEGATIVE ORDER ITEM
            if (item.Insurance.GetValueOrDefault() != 0)
            {
                seq++;
                orderItems.Add(new OrderItemDto2
                {
                    OrderItemSeqId = seq.ToString("D4"),
                    ProductId = item.ProductId,
                    ProductName = $"تأمين - {item.ProductName}",
                    Quantity = 1m,
                    UnitPrice = -Math.Abs(item.Insurance.Value),
                    SubTotal = -Math.Abs(item.Insurance.Value),
                    UomId = item.UomId,
                    FacilityId = certificate.FacilityId,
                    ItemDescription = "تأمين مستحق",
                    OrderItemTypeId = "PROJECT_INSURANCE",
                    StatusId = "ITEM_CREATED",
                    CreatedStamp = stamp,
                    LastUpdatedStamp = stamp
                });
            }

            // ADDITIONAL INSURANCE AS NEGATIVE ORDER ITEM
            if (item.AdditionalInsurance.GetValueOrDefault() != 0)
            {
                seq++;
                orderItems.Add(new OrderItemDto2
                {
                    OrderItemSeqId = seq.ToString("D4"),
                    ProductId = item.ProductId,
                    ProductName = $"تأمين إضافي - {item.ProductName}",
                    Quantity = 1m,
                    UnitPrice = -Math.Abs(item.AdditionalInsurance.Value),
                    SubTotal = -Math.Abs(item.AdditionalInsurance.Value),
                    UomId = item.UomId,
                    FacilityId = certificate.FacilityId,
                    ItemDescription = "تأمين إضافي",
                    OrderItemTypeId = "PROJECT_ADDITIONAL_INSURANCE",
                    StatusId = "ITEM_CREATED",
                    CreatedStamp = stamp,
                    LastUpdatedStamp = stamp
                });
            }
        }
        else // SUPPLY_PROCUREMENT_CERTIFICATE
        {
            // MAIN ITEM: Keep exact Quantity + UnitPrice from frontend
            var baseTotal = item.Quantity * item.UnitPrice;

            orderItems.Add(new OrderItemDto2
            {
                OrderItemSeqId = mainSeqId,
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                SubTotal = baseTotal,
                UomId = item.UomId,
                FacilityId = certificate.FacilityId,
                ItemDescription = item.Description,
                OrderItemTypeId = "PROJECT_CERTIFICATE_ITEM",
                StatusId = "ITEM_CREATED",
                CreatedStamp = stamp,
                LastUpdatedStamp = stamp
            });

            // Add extras as adjustments
            if (item.Discount.GetValueOrDefault() != 0)
                orderAdjustments.Add(CreateAdjustment(mainSeqId, "DISCOUNT_ADJUSTMENT", -item.Discount.Value, item));
            if (item.TransportationExpenses.GetValueOrDefault() != 0)
                orderAdjustments.Add(CreateAdjustment(mainSeqId, "SHIPPING_CHARGES", item.TransportationExpenses.Value, item));
            if (item.Gratuities.GetValueOrDefault() != 0)
                orderAdjustments.Add(CreateAdjustment(mainSeqId, "MISCELLANEOUS_CHARGE", item.Gratuities.Value, item));
        }

        seq++; // Increment only after main item
    }

    // FINAL GRAND TOTAL = Sum of all OrderItems.SubTotal + All Adjustments
    var grandTotal = orderItems.Sum(i => i.SubTotal) + orderAdjustments.Sum(a => a.Amount);

    var orderDto = new OrderDto
    {
        OrderTypeId = "PURCHASE_ORDER",
        FromPartyId = fromPartyId,
        CurrencyUomId = await _productStoreService.GetProductStoreDefaultCurrencyId(),
        OrderDate = stamp,
        StatusId = "ORDER_CREATED",
        StatusDescription = "Created",
        InternalRemarks = $"Auto-generated from Certificate {newProjectCertificateSerial}",
        GrandTotal = grandTotal,  // ← NOW 100% CORRECT
        OrderItems = orderItems,
        OrderAdjustments = orderAdjustments
    };

    poResult = await _orderService.CreatePurchaseOrder(orderDto);
    if (poResult == null)
    {
        await transaction.RollbackAsync(cancellationToken);
        return Result<ProjectCertificateDto>.Failure("Failed to create purchase order");
    }

    generatedOrderId = poResult.OrderId;
}