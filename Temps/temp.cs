private async Task<object> SetUnitPriceAsLastPrice(OrderItemDto2 orderItem)
{
    // REFACTOR: Declare variables outside try block to ensure scope in catch block
    // Fixes "Cannot resolve symbol" errors by making variables accessible for logging
    string productSupplierId = null;
    string orderCurrency = null;
    DateTime? nowTimestamp = null;

    try
    {
        // Query OrderRoles for BILL_FROM_VENDOR party ID
        productSupplierId = await _context.OrderRoles
            .Where(x => x.OrderId == orderItem.OrderId && x.RoleTypeId == "BILL_FROM_VENDOR")
            .Select(x => x.PartyId)
            .OrderBy(x => x.PartyId) // REFACTOR: Add OrderBy to ensure predictable results
            .FirstOrDefaultAsync();

        if (string.IsNullOrEmpty(productSupplierId))
        {
            // REFACTOR: Return success if no supplier found, consistent with original logic
            // Simplifies flow by avoiding unnecessary processing
            return new { success = true };
        }

        // Query OrderHeaders for currency
        orderCurrency = await _context.OrderHeaders
            .Where(x => x.OrderId == orderItem.OrderId)
            .Select(x => x.CurrencyUom)
            .OrderBy(x => x.CurrencyUom) // REFACTOR: Add OrderBy to ensure predictable results
            .FirstOrDefaultAsync();

        // Query SupplierProducts for active record
        var supplierProduct = await _context.SupplierProducts
            .Where(ps => ps.ProductId == orderItem.ProductId &&
                         ps.PartyId == productSupplierId &&
                         ps.AvailableThruDate == null &&
                         ps.CurrencyUomId == orderCurrency)
            .OrderBy(ps => ps.AvailableFromDate) // REFACTOR: Add OrderBy to ensure predictable results
            .FirstOrDefaultAsync();

        // REFACTOR: Use DateTime.UtcNow with microsecond precision
        // Leverages DATETIME(6) column to minimize duplicate key collisions
        nowTimestamp = DateTime.UtcNow;

        if (supplierProduct != null)
        {
            // Update SupplierProduct if price differs
            if (orderItem.UnitPrice.HasValue && orderItem.UnitPrice != supplierProduct.LastPrice)
            {
                supplierProduct.AvailableThruDate = nowTimestamp;
                _context.SupplierProducts.Update(supplierProduct);

                // REFACTOR: Check for existing record with the new primary key
                // Handles rare duplicates despite microsecond precision
                var newSupplierProductKey = new
                {
                    ProductId = orderItem.ProductId,
                    PartyId = productSupplierId,
                    CurrencyUomId = orderCurrency,
                    AvailableFromDate = nowTimestamp
                };
                var existingDuplicate = await _context.SupplierProducts
                    .Where(ps => ps.ProductId == newSupplierProductKey.ProductId &&
                                 ps.PartyId == newSupplierProductKey.PartyId &&
                                 ps.CurrencyUomId == newSupplierProductKey.CurrencyUomId &&
                                 ps.AvailableFromDate == newSupplierProductKey.AvailableFromDate)
                    .OrderBy(ps => ps.AvailableFromDate) // REFACTOR: Add OrderBy to ensure predictable results
                    .FirstOrDefaultAsync();

                if (existingDuplicate != null)
                {
                    // REFACTOR: Update existing record instead of failing
                    // Handles duplicates by updating LastPrice, avoiding key violation
                    _logger.LogInformation(
                        "Duplicate SupplierProduct found: ProductId: {ProductId}, PartyId: {PartyId}, CurrencyUomId: {CurrencyUomId}, AvailableFromDate: {AvailableFromDate}. Updating LastPrice.",
                        orderItem.ProductId, productSupplierId, orderCurrency, nowTimestamp);
                    existingDuplicate.LastPrice = orderItem.UnitPrice.Value;
                    existingDuplicate.AvailableThruDate = null; // Keep it active
                    _context.SupplierProducts.Update(existingDuplicate);
                }
                else
                {
                    // Create new SupplierProduct if no duplicate exists
                    var newSupplierProduct = new SupplierProduct
                    {
                        ProductId = orderItem.ProductId,
                        PartyId = productSupplierId,
                        CurrencyUomId = orderCurrency,
                        AvailableFromDate = (DateTime)nowTimestamp,
                        LastPrice = orderItem.UnitPrice.Value
                    };
                    _context.SupplierProducts.Add(newSupplierProduct);
                }

                await _context.SaveChangesAsync();
            }
        }
        else
        {
            // Create new SupplierProduct if no active record exists
            if (orderItem.UnitPrice.HasValue)
            {
                // REFACTOR: Check for existing record with the new primary key
                // Handles rare duplicates despite microsecond precision
                var newSupplierProductKey = new
                {
                    ProductId = orderItem.ProductId,
                    PartyId = productSupplierId,
                    CurrencyUomId = orderCurrency,
                    AvailableFromDate = nowTimestamp
                };
                var existingDuplicate = await _context.SupplierProducts
                    .Where(ps => ps.ProductId == newSupplierProductKey.ProductId &&
                                 ps.PartyId == newSupplierProductKey.PartyId &&
                                 ps.CurrencyUomId == newSupplierProductKey.CurrencyUomId &&
                                 ps.AvailableFromDate == newSupplierProductKey.AvailableFromDate)
                    .OrderBy(ps => ps.AvailableFromDate) // REFACTOR: Add OrderBy to ensure predictable results
                    .FirstOrDefaultAsync();

                if (existingDuplicate != null)
                {
                    // REFACTOR: Update existing record instead of failing
                    // Handles duplicates by updating LastPrice, avoiding key violation
                    _logger.LogInformation(
                        "Duplicate SupplierProduct found: ProductId: {ProductId}, PartyId: {PartyId}, CurrencyUomId: {CurrencyUomId}, AvailableFromDate: {AvailableFromDate}. Updating LastPrice.",
                        orderItem.ProductId, productSupplierId, orderCurrency, nowTimestamp);
                    existingDuplicate.LastPrice = orderItem.UnitPrice.Value;
                    existingDuplicate.AvailableThruDate = null; // Keep it active
                    _context.SupplierProducts.Update(existingDuplicate);
                }
                else
                {
                    // Create new SupplierProduct if no duplicate exists
                    var newSupplierProduct = new SupplierProduct
                    {
                        ProductId = orderItem.ProductId,
                        PartyId = productSupplierId,
                        CurrencyUomId = orderCurrency,
                        AvailableFromDate = (DateTime)nowTimestamp,
                        LastPrice = orderItem.UnitPrice.Value
                    };
                    _context.SupplierProducts.Add(newSupplierProduct);
                }

                await _context.SaveChangesAsync();
            }
        }

        // Relies on caller's transaction for commit or rollback
        return new { success = true };
    }
    catch (DbUpdateException ex) when (ex.InnerException is MySqlException mysqlEx && mysqlEx.Number == 1062)
    {
        // Uses variables declared outside try block for logging
        _logger.LogError(ex,
            "Unexpected duplicate key error for SupplierProduct with ProductId: {ProductId}, PartyId: {PartyId}, CurrencyUomId: {CurrencyUomId}, AvailableFromDate: {AvailableFromDate}",
            orderItem.ProductId, productSupplierId ?? "N/A", orderCurrency ?? "N/A", nowTimestamp);
        return new { success = true }; // Still return success to avoid failing caller's transaction
    }
    catch (Exception ex)
    {
        // REFACTOR: Log unexpected errors and rethrow
        // Uses variables declared outside try block for logging
        _logger.LogError(ex,
            "Error processing SupplierProduct for ProductId: {ProductId}, PartyId: {PartyId}, CurrencyUomId: {CurrencyUomId}, AvailableFromDate: {AvailableFromDate}",
            orderItem.ProductId, productSupplierId ?? "N/A", orderCurrency ?? "N/A", nowTimestamp);
        throw;
    }
}