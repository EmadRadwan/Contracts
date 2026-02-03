public async Task<string> CreateAccountingTransactionForApartmentIncomingPayment(string paymentId)
{
    try
    {
        var payment = await _context.Payments
            .Include(p => p.PaymentMethod)
            .Include(p => p.SalesRequest)
            .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

        if (payment == null)
            throw new Exception($"Payment not found: {paymentId}");

        if (payment.SalesRequest == null)
            throw new Exception($"SalesRequest not found for payment: {paymentId}");

        var companyPartyId = await _productStoreService.GetProductStorePayToPartId();
        var now = DateTime.UtcNow;
        var transactionDate = payment.EffectiveDate;

        string bankOrCashGlAccountId = payment.PaymentMethod?.GlAccountId
                                       ?? throw new Exception("GL account not configured on payment method");

        // ────────────────────────────────────────────────
        // Determine payment type & related strings
        // ────────────────────────────────────────────────
        string paymentType;
        string paymentTypeDescription;
        string chequeOrTransferRef = string.Empty;

        if (payment.IsBankTransfer == true)
        {
            paymentType = "BankTransfer";
            paymentTypeDescription = "Bank Transfer";
        }
        else if (!string.IsNullOrEmpty(payment.ChequeNumber))
        {
            paymentType = "Cheque";
            paymentTypeDescription = "Cheque";
            chequeOrTransferRef = $"#{payment.ChequeNumber}";
        }
        else
        {
            paymentType = "Cash";
            paymentTypeDescription = "Cash";
        }

        // ────────────────────────────────────────────────
        // Determine credit GL account — with party-specific override
        // ────────────────────────────────────────────────
        string creditGlAccountId;

        if (payment.SalesRequest.IsChequesDelivered == true)
        {
            // For delivered cheques → always use Cheques Under Collection
            creditGlAccountId = "124410";
        }
        else
        {
            // For normal receivable → try party-specific → fallback to default
            creditGlAccountId = await GetCustomerReceivableGlAccountId(
                organizationPartyId: companyPartyId,
                customerPartyId:     payment.PartyIdFrom,
                cancellationToken:   default);
        }

        // Main transaction description
        var description = $"Apartment incoming payment - {paymentTypeDescription} {chequeOrTransferRef} - " +
                          $"Payment {payment.PaymentId} - SR {payment.SalesRequestId}";

        var acctgTransParams = new CreateAcctgTransParams
        {
            AcctgTransTypeId = "INCOMING_PAYMENT",
            TransactionDate  = transactionDate,
            IsPosted         = "Y",
            Description      = description,
            GlFiscalTypeId   = "ACTUAL",
            PaymentId        = payment.PaymentId,
            SalesRequestId   = payment.SalesRequestId,
            PartyId          = payment.PartyIdFrom
        };

        string acctgTransId = await _acctgTransService.CreateAcctgTrans(acctgTransParams);

        int seq = 0;

        // ────────────────────────────────────────────────
        // Debit entry (incoming money → bank/cash)
        // ────────────────────────────────────────────────
        string debitDescription = paymentType switch
        {
            "BankTransfer" => $"Bank transfer received from customer {chequeOrTransferRef}",
            "Cheque"       => $"Bank deposit - Cleared cheque {chequeOrTransferRef}",
            _              => "Cash receipt from customer"
        };

        var debitEntry = new AcctgTransEntry
        {
            AcctgTransId          = acctgTransId,
            AcctgTransEntrySeqId  = (++seq).ToString("D3"),
            GlAccountId           = bankOrCashGlAccountId,
            DebitCreditFlag       = "D",
            AcctgTransEntryTypeId = "_NA_",
            Amount                = payment.Amount,
            ReconcileStatusId     = "AES_NOT_RECONCILED",
            Description           = debitDescription,
            OrganizationPartyId   = companyPartyId,
            PartyId               = payment.PartyIdFrom,
            CreatedStamp          = now,
            LastUpdatedStamp      = now
        };
        await _acctgTransService.CreateAcctgTransEntry(debitEntry);

        // ────────────────────────────────────────────────
        // Credit entry (reducing receivable or cheques under collection)
        // ────────────────────────────────────────────────
        string creditDescription = payment.SalesRequest.IsChequesDelivered == true
            ? paymentType switch
            {
                "BankTransfer" => $"Bank transfer applied - reducing cheques under collection",
                "Cheque"       => $"Clearing cheques under collection {chequeOrTransferRef}",
                _              => $"Cash payment applied - reducing cheques under collection"
            }
            : $"Receipt against customer receivable {chequeOrTransferRef}";

        var creditEntry = new AcctgTransEntry
        {
            AcctgTransId          = acctgTransId,
            AcctgTransEntrySeqId  = (++seq).ToString("D3"),
            GlAccountId           = creditGlAccountId,
            DebitCreditFlag       = "C",
            AcctgTransEntryTypeId = "_NA_",
            Amount                = payment.Amount,
            ReconcileStatusId     = "AES_NOT_RECONCILED",
            Description           = creditDescription,
            OrganizationPartyId   = companyPartyId,
            PartyId               = payment.PartyIdFrom,
            CreatedStamp          = now,
            LastUpdatedStamp      = now
        };
        await _acctgTransService.CreateAcctgTransEntry(creditEntry);

        _logger.LogInformation(
            "Accounting transaction {AcctgTransId} created for apartment incoming {PaymentType} payment {PaymentId}. " +
            "Debit: {DebitDesc} | Credit GL {CreditGlAccountId} (IsChequesDelivered: {IsChequesDelivered})",
            acctgTransId, paymentTypeDescription, paymentId,
            debitDescription, creditGlAccountId, payment.SalesRequest.IsChequesDelivered);

        return acctgTransId;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to create accounting transaction for apartment incoming payment {PaymentId}",
            paymentId);
        throw;
    }
}