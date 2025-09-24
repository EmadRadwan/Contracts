private async Task SetUnitPriceAsLastPrice(OrderItemDto2 orderItem)
{
    var orderRole = await _utilityService.FindLocalOrDatabaseAsync2<OrderRole>(
        x => x.OrderId == orderItem.OrderId && x.RoleTypeId == "BILL_FROM_VENDOR");
    var productSupplierId = orderRole?.PartyId;

    var orderHeader = await _utilityService.FindLocalOrDatabaseAsync2<OrderHeader>(
        x => x.OrderId == orderItem.OrderId);
    var orderCurrency = orderHeader?.CurrencyUom;

    // get product supplier
    var selectedProductSuppliers = _context.SupplierProducts
        .Where(ps => ps.ProductId == orderItem.ProductId
                     && ps.PartyId == productSupplierId && ps.AvailableThruDate == null &&
                     ps.CurrencyUomId == orderCurrency)
        .ToList();


    foreach (var supplierProduct in selectedProductSuppliers)
    {
        var nowTimestamp = DateTime.Now;

        if (orderItem.UnitPrice != supplierProduct.LastPrice)
        {
            var newSupplierProduct = CloneSupplierProduct(supplierProduct);
            newSupplierProduct.AvailableFromDate = nowTimestamp;
            newSupplierProduct.LastPrice = orderItem.UnitPrice;
            _context.Add(newSupplierProduct);

            supplierProduct.AvailableThruDate = nowTimestamp;
        }
    }
}