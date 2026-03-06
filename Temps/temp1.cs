// ────────────────────────────────────────────────────────────────
// Helper: Returns signed amount for one invoice item
// ────────────────────────────────────────────────────────────────
private async Task<decimal> GetSignedItemAmount(InvoiceItem item)
{
    try
    {
        var quantity = item.Quantity ?? 1m;
        var baseAmount = item.Amount ?? 0m;

        // Load the type classification (include navigation property)
        // Assumption: InvoiceItem has navigation property InvoiceItemType (or you eager-load it)
        var itemType = await _context.InvoiceItemTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.InvoiceItemTypeId == item.InvoiceItemTypeId);

        bool isPositive = itemType?.IsPositiveAmount ?? true; // ← default to positive when null/old

        var signedAmount = isPositive ? baseAmount : -baseAmount;

        var lineTotal = quantity * signedAmount;

        return decimal.Round(lineTotal, 2, MidpointRounding.AwayFromZero);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, $"Error determining signed amount for item {item.InvoiceItemSeqId}");
        throw;
    }
}

// Alternative lightweight version without per-item DB call (recommended if possible)
private decimal GetSignedItemAmountNoDb(InvoiceItem item, Dictionary<string, bool?> typeCache)
{
    var quantity = item.Quantity ?? 1m;
    var baseAmount = item.Amount ?? 0m;

    bool isPositive = typeCache.TryGetValue(item.InvoiceItemTypeId, out var flag) 
        ? (flag ?? true) 
        : true; // default positive

    var signedAmount = isPositive ? baseAmount : -baseAmount;
    return decimal.Round(quantity * signedAmount, 2, MidpointRounding.AwayFromZero);
}

// ────────────────────────────────────────────────────────────────
// Main method – refactored
// ────────────────────────────────────────────────────────────────
public async Task<decimal> GetInvoiceTotal(string invoiceId, bool actualCurrency)
{
    decimal invoiceTotal = 0m;

    try
    {
        var taxableItemTypeIds = await GetTaxableInvoiceItemTypeIds();

        // Include InvoiceItemType navigation (or use projection + cache)
        var invoiceItems = await _context.InvoiceItems
            .AsNoTracking()
            .Where(ii => ii.InvoiceId == invoiceId 
                      && !taxableItemTypeIds.Contains(ii.InvoiceItemTypeId))
            .Include(ii => ii.InvoiceItemType)  // ← add this if navigation exists
            .Select(ii => new 
            {
                ii.InvoiceItemTypeId,
                ii.Quantity,
                ii.Amount,
                IsPositiveAmount = ii.InvoiceItemType != null 
                    ? ii.InvoiceItemType.IsPositiveAmount 
                    : (bool?)null
            })
            .ToListAsync();

        if (invoiceItems.Any())
        {
            bool hasCertificateItem = invoiceItems.Any(ii => ii.InvoiceItemTypeId == "PINV_CERTIFICATE_ITEM");
            bool hasCertificateSupplyItem = invoiceItems.Any(ii => ii.InvoiceItemTypeId == "PINV_CERTIFICATE_SUPPLY_ITEM");

            decimal baseTotal;

            if (hasCertificateItem)
            {
                // Rule 1 – Certificate invoice: certificate amount(s) MINUS everything else
                var certificateTotal = invoiceItems
                    .Where(ii => ii.InvoiceItemTypeId == "PINV_CERTIFICATE_ITEM")
                    .Sum(ii => (ii.Quantity ?? 1m) * (ii.Amount ?? 0m) * ((ii.IsPositiveAmount ?? true) ? 1m : -1m));

                var otherTotal = invoiceItems
                    .Where(ii => ii.InvoiceItemTypeId != "PINV_CERTIFICATE_ITEM")
                    .Sum(ii => (ii.Quantity ?? 1m) * (ii.Amount ?? 0m) * ((ii.IsPositiveAmount ?? true) ? 1m : -1m));

                baseTotal = certificateTotal - otherTotal;
            }
            else if (hasCertificateSupplyItem)
            {
                // Rule 2 – Certificate-Supply: normal signed sum
                baseTotal = invoiceItems.Sum(ii => 
                    (ii.Quantity ?? 1m) * (ii.Amount ?? 0m) * ((ii.IsPositiveAmount ?? true) ? 1m : -1m));
            }
            else
            {
                // Rule 3 – Regular: normal signed sum of non-taxable items
                baseTotal = invoiceItems.Sum(ii => 
                    (ii.Quantity ?? 1m) * (ii.Amount ?? 0m) * ((ii.IsPositiveAmount ?? true) ? 1m : -1m));
            }

            invoiceTotal = baseTotal;
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, $"Error retrieving invoice items for invoice {invoiceId}.");
        throw new Exception("Error retrieving invoice items.", ex);
    }

    try
    {
        // Tax (unchanged – assuming tax is always positive/additive)
        var invoiceTaxTotal = await GetInvoiceTaxTotal(invoiceId);
        invoiceTotal += invoiceTaxTotal;

        // Currency conversion (unchanged – commented block preserved)
        /*
        if (invoiceTotal != 0 && !actualCurrency)
        {
            var invoice = await _context.Invoices.FindAsync(invoiceId);
            if (invoice == null) { ... }
            invoiceTotal *= await GetInvoiceCurrencyConversionRate(invoice);
        }
        */
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, $"Error performing tax or currency conversion for invoice {invoiceId}.");
        throw new Exception("Error performing tax/currency.", ex);
    }

    return Math.Round(invoiceTotal, 2, MidpointRounding.AwayFromZero);
}