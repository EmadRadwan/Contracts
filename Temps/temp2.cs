if (statusId == "PMNT_RECEIVED" && oldStatusId != "PMNT_RECEIVED")
{
    try
    {
        bool isChequeInstallmentWithEarlyTrans = !string.IsNullOrEmpty(payment.SalesRequestId) &&
                                                 !string.IsNullOrEmpty(payment.ChequeNumber) &&
                                                 await _context.AcctgTrans.AnyAsync(at => 
                                                     at.PaymentId == paymentId && 
                                                     at.AcctgTransEntries.Any(e => e.GlAccountId == "124410"));

        if (isChequeInstallmentWithEarlyTrans)
        {
            // Special path: Cheque cleared → move from Cheques Under Collection to Bank
            await _generalLedgerService.CreateChequeClearanceAccountingTransaction(paymentId);

            // Still do these (required for proper application)
            await _invoiceService.CheckPaymentInvoices(paymentId);
            await CreateMatchingPaymentApplication(paymentId, null);

            // Skip everything else in this block
            // → Just continue to next ECA (PMNT_SENT, etc.)
        }
        else
        {
            // Normal path: Direct receipt (cash, transfer, or first-time cheque without early trans)
            if (oldStatusId != "PMNT_CONFIRMED")
            {
                await _generalLedgerService.CreateAcctgTransAndEntriesForIncomingPayment(paymentId);
            }

            await _invoiceService.CheckPaymentInvoices(paymentId);
            await CreateMatchingPaymentApplication(paymentId, null);
        }
    }
    catch (Exception ex)
    {
        return new PaymentStatusChangeResult
        {
            Success = false,
            ErrorCode = "ECA_LOGIC_FAILED",
            ErrorMessage = $"Failed to process accounting for received payment: {ex.Message}"
        };
    }
}