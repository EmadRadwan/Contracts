if (statusId == "PMNT_SENT" && oldStatusId != "PMNT_SENT")
{
    try
    {
        // ───────────────────────────────────────────────
        // Fetch required data once (avoid multiple queries)
        // ───────────────────────────────────────────────
        var paymentType = await _context.PaymentTypes
            .FirstOrDefaultAsync(pt => pt.PaymentTypeId == payment.PaymentTypeId);

        bool isPostDatedCheque = 
            !string.IsNullOrEmpty(payment.OverrideGlAccountId) &&
            paymentType?.ParentTypeId == "DISBURSEMENT" &&
            (!string.IsNullOrEmpty(payment.ChequeNumber) || payment.ChequeDate.HasValue);

        if (isPostDatedCheque)
        {
            // Special accounting: debit POSTDATED CHECKS ISSUED, credit Bank
            await _generalLedgerService.CreatePostdatedChequeIssuedAccountingTransaction(payment.PaymentId);
        }
        else
        {
            // Normal outgoing payment accounting
            if (oldStatusId != "PMNT_CONFIRMED")
            {
                await _generalLedgerService.CreateAcctgTransAndEntriesForOutgoingPayment(payment.PaymentId);
            }
        }

        //await _invoiceService.CheckPaymentInvoices(paymentId);
        //await CreateMatchingPaymentApplication(paymentId, null);
    }
    catch (Exception ex)
    {
        return new PaymentStatusChangeResult
        {
            Success = false,
            ErrorCode = "ECA_LOGIC_FAILED",
            ErrorMessage = $"Failed to process accounting transactions: {ex.Message}"
        };
    }
}