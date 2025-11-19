if (certificate.CertificateCategory != "COMPANY_SUPPLY_SALE_CERTIFICATE")
{
    var orderItems = new List<OrderItemDto2>();
    int seq = 1;

    string fromPartyId = certificate.CertificateCategory switch
    {
        "SUPPLY_PROCUREMENT_CERTIFICATE" => certificate.PartyIdSupplier 
            ?? throw new InvalidOperationException("PartyIdSupplier is required for supply/procurement certificates"),
        "WORKMANSHIP_CONTRACTING_CERTIFICATE" => certificate.PartyIdContractor 
            ?? throw new InvalidOperationException("PartyIdContractor is required for workmanship/contracting certificates"),
        _ => throw new InvalidOperationException($"Unsupported certificate category: {certificate.CertificateCategory}")
    };

    foreach (var item in certificate.CertificateItems!)
    {
        // Main payable line — always use frontend-calculated net
        decimal netAmount = item.Net ?? 0;

        orderItems.Add(new OrderItemDto2
        {
            OrderItemSeqId = seq.ToString("D4"),
            ProductId = item.ProductId,
            ProductName = item.ProductName,
            Quantity = item.Quantity,
            UnitPrice = item.Quantity > 0 ? netAmount / item.Quantity : netAmount,
            SubTotal = netAmount,
            UomId = item.UomId,
            FacilityId = certificate.FacilityId,
            ItemDescription = item.Description,
            OrderItemTypeId = "PROJECT_CERTIFICATE_ITEM",
            StatusId = "ITEM_CREATED",
            CreatedStamp = stamp,
            LastUpdatedStamp = stamp
        });
        seq++;

        // Only for WORKMANSHIP: Add visible deduction lines for Insurance & Additional Insurance
        if (certificate.CertificateCategory == "WORKMANSHIP_CONTRACTING_CERTIFICATE")
        {
            if (item.Insurance.GetValueOrDefault() != 0)
            {
                orderItems.Add(new OrderItemDto2
                {
                    OrderItemSeqId = seq.ToString("D4"),
                    ProductId = item.ProductId,
                    ProductName = $"تأمين - {item.ProductName}",
                    Quantity = 1,
                    UnitPrice = -Math.Abs(item.Insurance!.Value),
                    SubTotal = -Math.Abs(item.Insurance.Value),
                    UomId = item.UomId,
                    FacilityId = certificate.FacilityId,
                    ItemDescription = "تأمين مستحق",
                    OrderItemTypeId = "INSURANCE_DEDUCTION_ITEM",
                    StatusId = "ITEM_CREATED",
                    CreatedStamp = stamp,
                    LastUpdatedStamp = stamp
                });
                seq++;
            }

            if (item.AdditionalInsurance.GetValueOrDefault() != 0)
            {
                orderItems.Add(new OrderItemDto2
                {
                    OrderItemSeqId = seq.ToString("D4"),
                    ProductId = item.ProductId,
                    ProductName = $"تأمين إضافي - {item.ProductName}",
                    Quantity = 1,
                    UnitPrice = -Math.Abs(item.AdditionalInsurance!.Value),
                    SubTotal = -Math.Abs(item.AdditionalInsurance.Value),
                    UomId = item.UomId,
                    FacilityId = certificate.FacilityId,
                    ItemDescription = "تأمين إضافي",
                    OrderItemTypeId = "ADDITIONAL_INSURANCE_DEDUCTION_ITEM",
                    StatusId = "ITEM_CREATED",
                    CreatedStamp = stamp,
                    LastUpdatedStamp = stamp
                });
                seq++;
            }
        }

        // Optional: Add positive lines for transparency (Transportation, Gratuities, etc.)
        // Only if you want them visible in PO — otherwise skip (since already in net)
        // Example:
        // if (item.TransportationExpenses.GetValueOrDefault() > 0) { ... }
        // if (item.Gratuities.GetValueOrDefault() > 0) { ... }
    }

    var grandTotal = orderItems.Sum(i => i.SubTotal);

    var orderDto = new OrderDto
    {
        OrderTypeId = "PURCHASE_ORDER",
        FromPartyId = fromPartyId,
        CurrencyUomId = await _productStoreService.GetProductStoreDefaultCurrencyId(),
        OrderDate = stamp,
        StatusId = "ORDER_CREATED",
        StatusDescription = "Created",
        InternalRemarks = $"Auto-generated from Certificate {newProjectCertificateSerial}",
        GrandTotal = grandTotal,
        OrderItems = orderItems,
        OrderAdjustments = new List<OrderAdjustmentDto2>() // Empty — fully removed
    };

    poResult = await _orderService.CreatePurchaseOrder(orderDto);
    if (poResult == null)
    {
        await transaction.RollbackAsync(cancellationToken);
        return Result<ProjectCertificateDto>.Failure("Failed to create purchase order");
    }

    generatedOrderId = poResult.OrderId;
}