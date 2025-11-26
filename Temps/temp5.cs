public async Task<decimal> GetInvoiceTotal(string invoiceId, bool actualCurrency)
{
    decimal invoiceTotal = 0m;

    try
    {
        var taxableItemTypeIds = await GetTaxableInvoiceItemTypeIds();

        // REFACTOR: Load InvoiceItemTypeId as well to detect certificate items
        var invoiceItems = await _utilityService.FindLocalOrDatabaseListAsync<InvoiceItem>(
            query => query.Where(ii => ii.InvoiceId == invoiceId 
                                    && !taxableItemTypeIds.Contains(ii.InvoiceItemTypeId))
                          .Select(ii => new InvoiceItem
                          {
                              InvoiceItemTypeId = ii.InvoiceItemTypeId,
                              Quantity = ii.Quantity,
                              Amount = ii.Amount
                          }));

        if (invoiceItems != null && invoiceItems.Any())
        {
            // REFACTOR: Detect certificate items
            var certificateItems = invoiceItems
                .Where(ii => ii.InvoiceItemTypeId == "PINV_CERTIFICATE_ITEM")
                .ToList();

            var nonCertificateItems = invoiceItems
                .Where(ii => ii.InvoiceItemTypeId != "PINV_CERTIFICATE_ITEM")
                .ToList();

            decimal baseTotal;

            if (certificateItems.Any())
            {
                // Sum only certificate items (rounded per line)
                var certificateTotal = certificateItems.Sum(item =>
                    GetInvoiceItemTotal(item)); // Uses same rounding logic

                // Sum all other non-certificate, non-taxable items
                var otherItemsTotal = nonCertificateItems.Sum(item =>
                    GetInvoiceItemTotal(item));

                // Final base = Certificate Total - Other Items Total
                baseTotal = certificateTotal - otherItemsTotal;
            }
            else
            {
                // Original behavior: sum all non-taxable items
                baseTotal = invoiceItems.Sum(item => GetInvoiceItemTotal(item));
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
        // Add tax (unchanged)
        var invoiceTaxTotal = await GetInvoiceTaxTotal(invoiceId);
        invoiceTotal += invoiceTaxTotal;

        // Currency conversion (unchanged)
        if (invoiceTotal != 0 && !actualCurrency)
        {
            var invoice = await _context.Invoices.FindAsync(invoiceId);
            if (invoice == null)
            {
                var errorMessage = $"Invoice with ID {invoiceId} not found for currency conversion.";
                _logger.LogError(errorMessage);
                throw new Exception(errorMessage);
            }

            invoiceTotal *= await GetInvoiceCurrencyConversionRate(invoice);
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, $"Error performing currency conversion or tax calculation for invoice {invoiceId}.");
        throw new Exception("Error performing currency conversion or tax.", ex);
    }

    // REFACTOR: Final rounding moved here (consistent with previous handler)
    return Math.Round(invoiceTotal, 2, MidpointRounding.AwayFromZero);
}