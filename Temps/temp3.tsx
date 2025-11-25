// ECA: Accounting transactions for INVOICE_READY
if (!string.IsNullOrEmpty(invoiceId) && statusId == "INVOICE_READY" && oldStatusId != "INVOICE_READY" && oldStatusId != "INVOICE_PAID")
{
    var ledgerService = _generalLedgerService.Value;
    try
    {
        // ——— SMART DETECTION WITHOUT InvoiceItems navigation ———
        bool isConstructionCertificate = false;

        // 1. Check description (you already set this in CreateProjectCertificate)
        if (!string.IsNullOrWhiteSpace(invoice.Description))
            isConstructionCertificate = invoice.Description.Contains("WORKMANSHIP_CONTRACTING_CERTIFICATE", StringComparison.OrdinalIgnoreCase);

        // 2. Check ReferenceNumber format (e.g. "8-0001")
        if (!isConstructionCertificate && !string.IsNullOrWhiteSpace(invoice.ReferenceNumber))
            isConstructionCertificate = invoice.ReferenceNumber.Contains('-') &&
                int.TryParse(invoice.ReferenceNumber.Split('-').Last(), out _);

        // 3. Final safety net: check invoice items (fast existence check only)
        if (!isConstructionCertificate)
        {
            isConstructionCertificate = await _context.InvoiceItems
                .AnyAsync(ii => ii.InvoiceId == invoiceId &&
                    ii.InvoiceItemTypeId == "PINV_CERTIFICATE_ITEM");
        }

        // ——— ROUTE TO CORRECT ACCOUNTING FUNCTION ———
        if (invoiceTypeId == "PURCHASE_INVOICE" && isConstructionCertificate)
        {
            _logger.LogInformation("Construction certificate invoice {InvoiceId} → using custom accounting", invoiceId);
            await ledgerService.CreateAcctgTransForConstructionCertificateInvoice(invoiceId);
        }
        else if (invoiceTypeId != "CUST_RTN_INVOICE")
        {
            await ledgerService.CreateAcctgTransForPurchaseInvoice(invoiceId);
            await ledgerService.CreateAcctgTransForSalesInvoice(invoiceId);
        }
        else if (invoiceTypeId == "CUST_RTN_INVOICE")
        {
            await ledgerService.CreateAcctgTransForCustomerReturnInvoice(invoiceId);
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to create accounting transactions for invoice {InvoiceId}", invoiceId);
        // Don't throw — status change must succeed even if accounting fails temporarily
    }

    // Rest of your logic...
    try
    {
        await _invoiceService.Value.CheckInvoicePaymentApplications(invoiceId);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, $"Failed to check or capture payments for invoice {invoiceId}");
    }
}