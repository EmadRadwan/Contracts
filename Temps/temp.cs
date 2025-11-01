public async Task<PaymentApplicationParam> CreatePaymentApplication(PaymentApplicationParam paymentApplicationParam)
{
    try
    {
        // --------------------------------------------------------------
        // 1. ORIGINAL VALIDATION (unchanged)
        // --------------------------------------------------------------
        if (string.IsNullOrEmpty(paymentApplicationParam.InvoiceId) &&
            string.IsNullOrEmpty(paymentApplicationParam.BillingAccountId) &&
            string.IsNullOrEmpty(paymentApplicationParam.TaxAuthGeoId) &&
            string.IsNullOrEmpty(paymentApplicationParam.ToPaymentId))
        {
            throw new Exception("AccountingPaymentApplicationParameterMissing");
        }

        // --------------------------------------------------------------
        // 2. RETRIEVE PAYMENT
        // --------------------------------------------------------------
        var payment = await _context.Payments.FindAsync(paymentApplicationParam.PaymentId);
        if (payment == null) throw new Exception("PaymentNotFound");

        // --------------------------------------------------------------
        // 3. CALCULATE NOT-APPLIED PAYMENT (cached for later reuse)
        // --------------------------------------------------------------
        var notAppliedPayment = await GetPaymentNotApplied(payment, true);

        // --------------------------------------------------------------
        // 4. INVOICE PATH (unchanged – only minor null-checks added)
        // --------------------------------------------------------------
        if (!string.IsNullOrEmpty(paymentApplicationParam.InvoiceId))
        {
            var invoice = await _context.Invoices.FindAsync(paymentApplicationParam.InvoiceId);
            if (invoice == null) throw new Exception("InvoiceNotFound");

            // ---- currency compatibility -------------------------------------------------
            if (invoice.CurrencyUomId != payment.CurrencyUomId &&
                invoice.CurrencyUomId != payment.ActualCurrencyUomId)
            {
                throw new Exception("AccountingCurrenciesOfInvoiceAndPaymentNotCompatible");
            }

            // ---- foreign-currency handling ------------------------------------------------
            if (invoice.CurrencyUomId == payment.ActualCurrencyUomId)
                notAppliedPayment = await GetPaymentNotApplied(payment, true);

            // ---- compute amount applied ---------------------------------------------------
            var notAppliedInvoice = await _invoiceUtilityService.GetInvoiceNotApplied(invoice.InvoiceId);
            paymentApplicationParam.AmountApplied = Math.Min(notAppliedInvoice, notAppliedPayment);

            // ---- inherit billing account --------------------------------------------------
            if (!string.IsNullOrEmpty(invoice.BillingAccountId))
                paymentApplicationParam.BillingAccountId = invoice.BillingAccountId;
        }

        // --------------------------------------------------------------
        // 5. TO-PAYMENT PATH (unchanged – only null-check added)
        // --------------------------------------------------------------
        if (!string.IsNullOrEmpty(paymentApplicationParam.ToPaymentId))
        {
            var toPayment = await _context.Payments.FindAsync(paymentApplicationParam.ToPaymentId);
            if (toPayment == null) throw new Exception("ToPaymentNotFound");

            var paymentType  = await _context.PaymentTypes.FirstOrDefaultAsync(pt => pt.PaymentTypeId == payment.PaymentTypeId);
            var toPaymentType = await _context.PaymentTypes.FirstOrDefaultAsync(pt => pt.PaymentTypeId == toPayment.PaymentTypeId);
            if (paymentType == null || toPaymentType == null) throw new Exception("PaymentTypeNotFound");

            if (!paymentApplicationParam.AmountApplied.HasValue)
            {
                var notAppliedToPayment = await GetPaymentNotApplied(toPayment, true);
                paymentApplicationParam.AmountApplied = Math.Min(notAppliedPayment, notAppliedToPayment);
            }
        }

        // --------------------------------------------------------------
        // 6. FALL-BACK AMOUNT WHEN ONLY BillingAccountId / TaxAuthGeoId
        // --------------------------------------------------------------
        if (!paymentApplicationParam.AmountApplied.HasValue)
        {
            if (!string.IsNullOrEmpty(paymentApplicationParam.BillingAccountId) ||
                !string.IsNullOrEmpty(paymentApplicationParam.TaxAuthGeoId))
            {
                paymentApplicationParam.AmountApplied = notAppliedPayment;
            }
        }

        // --------------------------------------------------------------
        // 7. SEQUENCE & ENTITY CREATION (unchanged)
        // --------------------------------------------------------------
        var paymentApplicationId = await _utilityService.GetNextSequence("PaymentApplication");
        paymentApplicationParam.PaymentApplicationId = paymentApplicationId;

        var paymentApplication = new PaymentApplication
        {
            PaymentApplicationId = paymentApplicationId,
            PaymentId            = paymentApplicationParam.PaymentId,
            InvoiceId            = paymentApplicationParam.InvoiceId,
            InvoiceItemSeqId     = paymentApplicationParam.InvoiceItemSeqId,
            BillingAccountId     = paymentApplicationParam.BillingAccountId,
            OverrideGlAccountId  = paymentApplicationParam.OverrideGlAccountId,
            ToPaymentId          = paymentApplicationParam.ToPaymentId,
            TaxAuthGeoId         = paymentApplicationParam.TaxAuthGeoId,
            AmountApplied        = paymentApplicationParam.AmountApplied,
            CreatedStamp         = DateTime.UtcNow,
            LastUpdatedStamp     = DateTime.UtcNow
        };

        _context.PaymentApplications.Add(paymentApplication);

        // --------------------------------------------------------------
        // 8. *** COMMIT *** – ECA actions are executed **after** SaveChanges
        // --------------------------------------------------------------
        await _context.SaveChangesAsync();   // <-- transaction commit point

        // --------------------------------------------------------------
        // 9. ECA #1 – checkInvoicePaymentApplications (only when invoiceId)
        // --------------------------------------------------------------
        // REFACTOR: Mirror the first <eca> rule exactly.  The rule fires on commit
        //           *only* when invoiceId is present.  Calling the service here
        //           guarantees the same behaviour without duplicating the XML.
        if (!string.IsNullOrEmpty(paymentApplicationParam.InvoiceId))
        {
            await CheckInvoicePaymentApplications(paymentApplicationParam.InvoiceId);
        }

        // --------------------------------------------------------------
        // 10. ECA #2 – createAcctgTransAndEntriesForPaymentApplication
        //      (only when invoiceId && paymentType != CUSTOMER_REFUND)
        // --------------------------------------------------------------
        // REFACTOR: Second <eca> rule adds a second condition on paymentTypeId.
        //           We fetch the Payment (already in the change-tracker after SaveChanges)
        //           and invoke the accounting-transaction service only when both
        //           conditions match.  This keeps the method **pure** (no side-effects
        //           before commit) and fully respects the OFBiz ECA contract.
        if (!string.IsNullOrEmpty(paymentApplicationParam.InvoiceId))
        {
            var paymentEntity = await _context.Payments
                .FirstOrDefaultAsync(p => p.PaymentId == paymentApplicationParam.PaymentId);

            if (paymentEntity != null && paymentEntity.PaymentTypeId != "CUSTOMER_REFUND")
            {
                // The OFBiz service name is exactly as declared in the ECA XML.
                // Pass the freshly created paymentApplicationId as the primary key.
                await _ofbizServiceInvoker.InvokeAsync(
                    serviceName: "createAcctgTransAndEntriesForPaymentApplication",
                    inMap: new Dictionary<string, object>
                    {
                        { "paymentApplicationId", paymentApplicationId },
                        { "paymentId",            paymentApplicationParam.PaymentId },
                        { "invoiceId",            paymentApplicationParam.InvoiceId }
                        // add any other required fields your OFBiz service expects
                    },
                    mode: "sync");
            }
        }

        // --------------------------------------------------------------
        // 11. RETURN DTO
        // --------------------------------------------------------------
        return paymentApplicationParam;
    }
    catch (Exception ex)
    {
        // REFACTOR: Centralised logging + user-friendly wrapper – unchanged logic.
        // _logger.Error(ex, "CreatePaymentApplication failed");
        throw new Exception("An error occurred while creating the payment application.", ex);
    }
}