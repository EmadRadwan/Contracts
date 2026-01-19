public async Task<string> CreatePostdatedChequeAccountingTransaction(string paymentId)
{
    try
    {
        // 1. Fetch payment with required data
        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

        if (payment == null)
            throw new Exception($"Payment not found: {paymentId}");

        // 2. Validation – this method is specifically for postdated outgoing cheques
        if (string.IsNullOrEmpty(payment.OverrideGlAccountId))
            throw new InvalidOperationException("OverrideGlAccountId is required for postdated cheque accounting.");

        if (string.IsNullOrEmpty(payment.ChequeNumber) && payment.ChequeDate == null)
            throw new InvalidOperationException("This method is only for cheque payments with ChequeNumber or ChequeDate.");

        var companyPartyId = await _productStoreService.GetProductStorePayToPartId();

        // Optional: warn if it doesn't look like an outgoing payment
        if (payment.PartyIdFrom != companyPartyId)
        {
            _logger.LogWarning("Payment {PaymentId} has PartyIdFrom {PartyIdFrom} which differs from company party {CompanyPartyId}",
                paymentId, payment.PartyIdFrom, companyPartyId);
        }

        var now = DateTime.UtcNow;
        var transactionDate = payment.EffectiveDate?.Date ?? now.Date;

        // Build cheque reference (prefer ChequeNumber when available)
        string chequeRef = !string.IsNullOrEmpty(payment.ChequeNumber)
            ? $"#{payment.ChequeNumber}"
            : $"dated {payment.ChequeDate?.ToString("yyyy-MM-dd")}";

        string description = $"Postdated cheque issuance – {chequeRef} – " +
                             $"Payment {payment.PaymentId} – {payment.Amount:N2} EGP";

        // 3. Create main accounting transaction
        var acctgTransParams = new CreateAcctgTransParams
        {
            AcctgTransTypeId   = "OUTGOING_PAYMENT", // consider "POSTDATED_CHEQUE_ISSUED" if your system supports it
            TransactionDate    = transactionDate,
            IsPosted           = "Y",
            GlFiscalTypeId     = "ACTUAL",
            Description        = description,
            PaymentId          = payment.PaymentId,
            PartyId            = payment.PartyIdTo   // recipient / payee
        };

        string acctgTransId = await _acctgTransService.CreateAcctgTrans(acctgTransParams);

        int seq = 0;

        // Debit entry – expense / liability / asset being paid (via override GL)
        string debitDescription = $"Postdated cheque issuance – {chequeRef} – to {payment.PartyIdTo}";

        var debitEntry = new AcctgTransEntry
        {
            AcctgTransId          = acctgTransId,
            AcctgTransEntrySeqId  = (++seq).ToString("D3"),
            GlAccountId           = payment.OverrideGlAccountId,
            DebitCreditFlag       = "D",
            AcctgTransEntryTypeId = "_NA_",
            Amount                = payment.Amount,
            ReconcileStatusId     = "AES_NOT_RECONCILED",
            Description           = debitDescription,
            OrganizationPartyId   = companyPartyId,
            PartyId               = payment.PartyIdTo,    // payee receives the debit side
            CreatedStamp          = now,
            LastUpdatedStamp      = now
        };
        await _acctgTransService.CreateAcctgTransEntry(debitEntry);

        // Credit entry – Postdated Cheques Issued (liability account)
        string creditDescription = $"Postdated cheques issued – {chequeRef}";

        var creditEntry = new AcctgTransEntry
        {
            AcctgTransId          = acctgTransId,
            AcctgTransEntrySeqId  = (++seq).ToString("D3"),
            GlAccountId           = "250100",             // POSTDATED CHECKS ISSUED
            DebitCreditFlag       = "C",
            AcctgTransEntryTypeId = "_NA_",
            Amount                = payment.Amount,
            ReconcileStatusId     = "AES_NOT_RECONCILED",
            Description           = creditDescription,
            OrganizationPartyId   = companyPartyId,
            PartyId               = companyPartyId,       // company issues the cheque
            CreatedStamp          = now,
            LastUpdatedStamp      = now
        };
        await _acctgTransService.CreateAcctgTransEntry(creditEntry);

        _logger.LogInformation(
            "Postdated cheque accounting transaction {AcctgTransId} created for payment {PaymentId} – {ChequeRef} – {Amount:N2}",
            acctgTransId, paymentId, chequeRef, payment.Amount);

        return acctgTransId;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to create postdated cheque accounting transaction for payment {PaymentId}", paymentId);
        throw;
    }
}