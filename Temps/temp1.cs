public async Task<ServiceResult> UpdateInvoiceItem(InvoiceItemParameters parameters)
{
    // 1. Fetch the existing InvoiceItem
    var invoiceItem = await _context.InvoiceItems
        .FirstOrDefaultAsync(ii => ii.InvoiceId == parameters.InvoiceId 
                                && ii.InvoiceItemSeqId == parameters.InvoiceItemSeqId);

    if (invoiceItem == null)
    {
        return new ServiceResult
        {
            IsError = true,
            ErrorMessage = "Invoice item not found."   // could use label/resource if you have localization
            // In real OFBiz style: label('AccountingUiLabels', 'AccountingInvoiceItemNotFound', parameters)
        };
    }

    // 2. Remember original values for change detection
    var original = new InvoiceItem
    {
        // copy only the fields we care about for comparison
        ProductId = invoiceItem.ProductId,
        // you can add more fields if needed for deeper comparison
    };

    // 3. Update non-PK fields from parameters (similar to setNonPKFields)
    // We do this manually since we don't have GenericValue.setNonPKFields
    if (!string.IsNullOrWhiteSpace(parameters.InvoiceItemTypeId))
        invoiceItem.InvoiceItemTypeId = parameters.InvoiceItemTypeId;

    if (!string.IsNullOrWhiteSpace(parameters.OverrideGlAccountId))
        invoiceItem.OverrideGlAccountId = parameters.OverrideGlAccountId;

    if (!string.IsNullOrWhiteSpace(parameters.Description))
        invoiceItem.Description = parameters.Description;

    if (parameters.Amount.HasValue)
        invoiceItem.Amount = parameters.Amount;

    if (!string.IsNullOrWhiteSpace(parameters.ProductId))
        invoiceItem.ProductId = parameters.ProductId;

    if (parameters.Quantity.HasValue)
        invoiceItem.Quantity = parameters.Quantity.Value;

    // add other updatable fields as needed...

    // 4. If productId changed → fetch product, update description & price
    bool productChanged = original.ProductId != invoiceItem.ProductId;

    if (productChanged && !string.IsNullOrWhiteSpace(invoiceItem.ProductId))
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.ProductId == invoiceItem.ProductId);

        if (product == null)
        {
            return new ServiceResult
            {
                IsError = true,
                ErrorMessage = $"Product not found: {invoiceItem.ProductId}"
            };
        }

        invoiceItem.Description = product.Description;

        // Mimic run service: calculateProductPrice
        // Adjust parameters according to your real _priceService interface
        var priceResult = await _priceService.CalculateProductPrice(
            product,
            quantity: invoiceItem.Quantity,
            // add user context, store, etc. if your real method needs them
            null, null, null
        );

        invoiceItem.Amount = priceResult.Price;

        if (!invoiceItem.Amount.HasValue)
        {
            return new ServiceResult
            {
                IsError = true,
                ErrorMessage = "Invoice amount is mandatory."  
                // matches: label('AccountingUiLabels', 'AccountingInvoiceAmountIsMandatory')
            };
        }
    }

    // 5. Only save if something actually changed
    bool hasChanges = productChanged ||
                      original.ProductId != invoiceItem.ProductId ||
                      // compare other updated fields as needed
                      // or use EF change tracking: _context.Entry(invoiceItem).State == EntityState.Modified
                      false; // placeholder — improve this

    if (hasChanges)
    {
        invoiceItem.LastUpdatedStamp = DateTime.UtcNow;

        try
        {
            // In real code: await _context.SaveChangesAsync();
            // Here we simulate success
            return new ServiceResult
            {
                IsError = false,
                Data = new 
                { 
                    invoiceId = invoiceItem.InvoiceId, 
                    invoiceItemSeqId = invoiceItem.InvoiceItemSeqId 
                }
            };
        }
        catch (Exception ex)
        {
            return new ServiceResult
            {
                IsError = true,
                ErrorMessage = $"Error updating invoice item: {ex.Message}"
            };
        }
    }

    // No changes → success with same keys
    return new ServiceResult
    {
        IsError = false,
        Data = new 
        { 
            invoiceId = invoiceItem.InvoiceId, 
            invoiceItemSeqId = invoiceItem.InvoiceItemSeqId 
        }
    };
}