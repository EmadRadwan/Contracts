// REFACTOR: Smart accounting transaction routing with explicit early-exit rules
// Priority order:
// 1. PINV_CERTIFICATE_SUPPLY_ITEM → internal supply certificate → NO accounting at all
// 2. PINV_CERTIFICATE_ITEM → WORKMANSHIP_CONTRACTING_CERTIFICATE → use custom accounting
// 3. All other invoices → standard OFBiz purchase/sales accounting
if (!string.IsNullOrEmpty(invoiceId) && 
    statusId == "INVOICE_READY" && 
    oldStatusId != "INVOICE_READY" && 
    oldStatusId != "INVOICE_PAID")
{
    var ledgerService = _generalLedgerService.Value;
    try
    {
        // ——— RULE 1: Check for supply certificate (PINV_CERTIFICATE_SUPPLY_ITEM) ———
        // These are internal material issuance certificates — no financial impact
        bool isSupplyCertificate = await _context.InvoiceItems
            .AnyAsync(ii => ii.InvoiceId == invoiceId && 
                           ii.InvoiceItemTypeId == "PINV_CERTIFICATE_SUPPLY_ITEM");

        if (isSupplyCertificate)
        {
            _logger.LogInformation(
                "Invoice {InvoiceId} is a supply certificate (PINV_CERTIFICATE_SUPPLY_ITEM) → skipping all accounting transactions",
                invoiceId);
            goto SkipAccounting; // ← EARLY EXIT: No accounting needed
        }

        // ——— RULE 2: Check for construction workmanship certificate ———
        bool isConstructionCertificate = await _context.InvoiceItems
            .AnyAsync(ii => ii.InvoiceId == invoiceId && 
                           ii.InvoiceItemTypeId == "PINV_CERTIFICATE_ITEM");

        if (invoiceTypeId == "PURCHASE_INVOICE" && isConstructionCertificate)
        {
            _logger.LogInformation(
                "Construction certificate detected (PINV_CERTIFICATE_ITEM) for invoice {InvoiceId} → using custom accounting logic",
                invoiceId);

            await ledgerService.CreateAcctgTransForConstructionCertificateInvoice(invoiceId);
        }
        else if (invoiceTypeId != "CUST_RTN_INVOICE")
        {
            // ——— FALLBACK: Normal purchase or sales invoices ———
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
        // Do NOT throw — invoice status change must succeed even if accounting fails
    }

    // Label to skip accounting entirely for supply certificates
    SkipAccounting:
    // Continue with payment application checks (always run)
    try
    {
        await _invoiceService.Value.CheckInvoicePaymentApplications(invoiceId);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to check payment applications for invoice {InvoiceId}", invoiceId);
    }
}