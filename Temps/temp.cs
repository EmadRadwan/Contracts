public async Task<string> CreateAcctgTransAndEntriesForPaymentApplication(string paymentApplicationId)
{
    try
    {
        // 1. Retrieve the PaymentApplication
        var paymentApplication = await _context.PaymentApplications
            .FirstOrDefaultAsync(pa => pa.PaymentApplicationId == paymentApplicationId);
        if (paymentApplication == null)
        {
            _logger.LogWarning($"PaymentApplication with ID {paymentApplicationId} was not found.");
            return null;
        }

        // 2. Retrieve the Payment associated with this PaymentApplication
        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.PaymentId == paymentApplication.PaymentId);
        if (payment == null)
        {
            _logger.LogWarning($"Payment with ID {paymentApplication.PaymentId} was not found.");
            return null;
        }

        // 3. If the payment transaction has not been posted to GL yet, do nothing
        if (payment.StatusId == "PMNT_NOT_PAID")
        {
            _logger.LogInformation(
                $"Payment {payment.PaymentId} not yet posted to GL. Skipping PaymentApplication {paymentApplicationId}.");
            return null;
        }

        // 4. Check if this is a 'RECEIPT' or a 'DISBURSEMENT'
        var parentPaymentType = await _utilityService.GetPaymentParentType(payment.PaymentTypeId);

        var acctgTransEntries = new List<AcctgTransEntry>();
        int seqNum = 1;
        AcctgTransEntry debitEntry = null;
        AcctgTransEntry creditEntry = null;

        // 5. If it is an incoming payment ("RECEIPT")
        if (parentPaymentType == "RECEIPT")
        {
            // 5.1. Check if the PaymentGlAccountTypeMap for the organization party is already ACCOUNTS_RECEIVABLE
            var paymentGlAccountTypeMap = await _context.PaymentGlAccountTypeMaps
                .FirstOrDefaultAsync(map => map.PaymentTypeId == payment.PaymentTypeId
                                         && map.OrganizationPartyId == payment.PartyIdTo);

            if (paymentGlAccountTypeMap?.GlAccountTypeId == "ACCOUNTS_RECEIVABLE")
            {
                _logger.LogInformation(
                    $"Payment {payment.PaymentId} credited account is 'ACCOUNTS_RECEIVABLE'. Skipping PaymentApplication {paymentApplicationId}.");
                return null;
            }

            // 5.2. Build the DEBIT entry (from unapplied → AR)
            debitEntry = new AcctgTransEntry
            {
                AcctgTransEntrySeqId = seqNum.ToString("D2"),
                AcctgTransEntryTypeId = "_NA_",
                ReconcileStatusId = "AES_NOT_RECONCILED",
                DebitCreditFlag = "D",
                OrganizationPartyId = payment.PartyIdTo,
                PartyId = payment.PartyIdFrom,
                RoleTypeId = "BILL_TO_CUSTOMER",
                OrigAmount = paymentApplication.AmountApplied,
                OrigCurrencyUomId = payment.CurrencyUomId,
                GlAccountId = payment.OverrideGlAccountId,
                GlAccountTypeId = paymentGlAccountTypeMap?.GlAccountTypeId  // e.g. ACCREC_UNAPPLIED
            };
            seqNum++;

            // 5.3. Build the CREDIT entry (to AR)
            creditEntry = new AcctgTransEntry
            {
                AcctgTransEntrySeqId = seqNum.ToString("D2"),
                AcctgTransEntryTypeId = "_NA_",
                ReconcileStatusId = "AES_NOT_RECONCILED",
                DebitCreditFlag = "C",
                OrganizationPartyId = payment.PartyIdTo,
                PartyId = payment.PartyIdFrom,
                RoleTypeId = "BILL_TO_CUSTOMER",
                OrigAmount = paymentApplication.AmountApplied,
                OrigCurrencyUomId = payment.CurrencyUomId,
                GlAccountTypeId = "ACCOUNTS_RECEIVABLE"  // Fixed liability account
            };
            seqNum++;

            acctgTransEntries.Add(debitEntry);
            acctgTransEntries.Add(creditEntry);
        }
        // 6. Otherwise, it's an outgoing payment ("DISBURSEMENT" or another type)
        else
        {
            // REFACTOR: Get the GL account type map for the *paying* organization
            // This tells us where the payment was *originally* parked (e.g. ACCPAYABLE_UNAPPLIED)
            var paymentGlAccountTypeMap = await _context.PaymentGlAccountTypeMaps
                .FirstOrDefaultAsync(map => map.PaymentTypeId == payment.PaymentTypeId
                                         && map.OrganizationPartyId == payment.PartyIdFrom);

            // REFACTOR: Skip only if the payment is *already* in the unapplied bucket
            // This prevents double-application (mirrors RECEIPT logic)
            if (paymentGlAccountTypeMap?.GlAccountTypeId == "ACCPAYABLE_UNAPPLIED")
            {
                _logger.LogInformation(
                    $"Payment {payment.PaymentId} is already in ACCPAYABLE_UNAPPLIED. Skipping PaymentApplication {paymentApplicationId}.");
                return null;
            }

            // 6.2. Get exchange rates for the invoice and the outgoing payment
            decimal? invoiceExchangeRate = await _acctgMiscService.GetGlExchangeRateOfPurchaseInvoice(paymentApplication);
            decimal? paymentExchangeRate = await _acctgMiscService.GetGlExchangeRateOfOutgoingPayment(paymentApplication);
            var ieRate = invoiceExchangeRate ?? 1.0m;
            var payRate = paymentExchangeRate ?? 1.0m;

            // REFACTOR: CREDIT → Remove from unapplied bucket (use mapped account)
            // This is the *parking* account — must come from the map to support multi-company
            creditEntry = new AcctgTransEntry
            {
                AcctgTransEntrySeqId = seqNum.ToString("D2"),
                AcctgTransEntryTypeId = "_NA_",
                ReconcileStatusId = "AES_NOT_RECONCILED",
                DebitCreditFlag = "C",                                 // CREDIT: clear unapplied
                OrganizationPartyId = payment.PartyIdFrom,
                PartyId = payment.PartyIdTo,
                RoleTypeId = "BILL_FROM_VENDOR",
                OrigAmount = paymentApplication.AmountApplied,
                OrigCurrencyUomId = payment.CurrencyUomId,
                Amount = paymentApplication.AmountApplied * payRate,
                GlAccountId = payment.OverrideGlAccountId,
                GlAccountTypeId = paymentGlAccountTypeMap?.GlAccountTypeId  // e.g. ACCPAYABLE_UNAPPLIED
            };
            acctgTransEntries.Add(creditEntry);
            seqNum++;

            // 6.4. If invoiceRate != paymentRate, add an FX gain/loss entry
            if (ieRate != payRate)
            {
                var fxGainLossEntry = new AcctgTransEntry
                {
                    AcctgTransEntrySeqId = seqNum.ToString("D2"),
                    AcctgTransEntryTypeId = "_NA_",
                    ReconcileStatusId = "AES_NOT_RECONCILED",
                    DebitCreditFlag = "D",
                    OrganizationPartyId = payment.PartyIdFrom,
                    PartyId = payment.PartyIdTo,
                    RoleTypeId = "BILL_FROM_VENDOR",
                    Amount = paymentApplication.AmountApplied * (payRate - ieRate),
                    GlAccountTypeId = "FX_GAIN_LOSS_ACCT"
                };
                acctgTransEntries.Add(fxGainLossEntry);
                seqNum++;
            }

            // REFACTOR: DEBIT → Reduce Accounts Payable
            // This is a *fixed liability account* — no map exists in OFBiz → safe to hardcode
            debitEntry = new AcctgTransEntry
            {
                AcctgTransEntrySeqId = seqNum.ToString("D2"),
                AcctgTransEntryTypeId = "_NA_",
                ReconcileStatusId = "AES_NOT_RECONCILED",
                DebitCreditFlag = "D",                                 // DEBIT: reduce liability
                OrganizationPartyId = payment.PartyIdFrom,
                PartyId = payment.PartyIdTo,
                RoleTypeId = "BILL_FROM_VENDOR",
                OrigAmount = paymentApplication.AmountApplied,
                Amount = paymentApplication.AmountApplied * ieRate,
                OrigCurrencyUomId = payment.CurrencyUomId,
                GlAccountTypeId = "ACCOUNTS_PAYABLE"                   // Fixed liability → safe
            };

            // 6.6. Override GL account or handle tax authority
            if (!string.IsNullOrEmpty(paymentApplication.OverrideGlAccountId))
            {
                debitEntry.GlAccountId = paymentApplication.OverrideGlAccountId;
            }
            else if (!string.IsNullOrEmpty(paymentApplication.TaxAuthGeoId))
            {
                var taxAuthorityGlAccount = await _context.TaxAuthorityGlAccounts
                    .FirstOrDefaultAsync(gl => gl.OrganizationPartyId == payment.PartyIdFrom
                                            && gl.TaxAuthGeoId == paymentApplication.TaxAuthGeoId
                                            && gl.TaxAuthPartyId == payment.PartyIdTo);
                if (taxAuthorityGlAccount != null)
                    debitEntry.GlAccountId = taxAuthorityGlAccount.GlAccountId;
            }

            acctgTransEntries.Add(debitEntry);
            seqNum++;
        }

        // 7. Prepare the parameters for creating the AcctgTrans (header)
        var createParams = new CreateAcctgTransAndEntriesParams
        {
            AcctgTransEntries = acctgTransEntries,
            AcctgTransTypeId = "PAYMENT_APPL",
            GlFiscalTypeId = "ACTUAL",
            PaymentId = paymentApplication.PaymentId,
            InvoiceId = paymentApplication.InvoiceId,
            PartyId = parentPaymentType == "RECEIPT" ? payment.PartyIdFrom : payment.PartyIdTo,
            RoleTypeId = parentPaymentType == "RECEIPT" ? "BILL_TO_CUSTOMER" : "BILL_FROM_VENDOR",
            TransactionDate = payment.EffectiveDate
        };

        // 8. Call your existing method to create the accounting transaction and entries
        var acctgTransId = await CreateAcctgTransAndEntries(createParams);

        // 9. Return the newly created accounting transaction ID
        return acctgTransId;
    }
    catch (Exception ex)
    {
        // 10. Error handling
        _logger.LogError(ex, $"Error creating accounting transaction for PaymentApplication {paymentApplicationId}");
        throw new Exception($"An error occurred while processing payment application {paymentApplicationId}", ex);
    }
}

EditMultiAcctgTrans