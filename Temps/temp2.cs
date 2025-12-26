public async Task<Result<CreateSalesRequest.SalesRequestResponseDto>> Handle(Command request, CancellationToken ct)
{
    var salesRequestId = request.SalesRequestId;

    await using var transaction = await _context.Database.BeginTransactionAsync(ct);

    try
    {
        // 1. Load SalesRequest with Installments
        var sr = await _context.SalesRequests
            .Include(s => s.Installments) // Critical: Load custom installments
            .FirstOrDefaultAsync(x => x.SalesRequestId == salesRequestId, ct);

        if (sr == null)
            return Result<CreateSalesRequest.SalesRequestResponseDto>.Failure("Sales request not found");

        if (sr.StatusId == "SALES_REQUEST_APPROVED")
            return Result<CreateSalesRequest.SalesRequestResponseDto>.Failure("Sales request is already approved");

        // 2. Validate that custom installments exist
        if (sr.Installments == null || !sr.Installments.Any())
        {
            return Result<CreateSalesRequest.SalesRequestResponseDto>.Failure(
                "Cannot approve: No payment plan defined. Please create and apply a custom payment plan.");
        }

        // 3. Update status
        sr.StatusId = "SALES_REQUEST_APPROVED";
        sr.LastUpdatedStamp = DateTime.UtcNow;

        // 4. Get company PayTo PartyId
        var companyPartyId = await _productStoreService.GetProductStorePayToPartId();

        // 5. Generate Payments from saved Installments
        var paymentsToCreate = new List<CreatePaymentParam>();

        // Sort by due date (and installment number as fallback) to ensure consistent order
        var orderedInstallments = sr.Installments
            .OrderBy(i => i.DueDate)
            .ThenBy(i => i.InstallmentNumber)
            .ToList();

        foreach (var inst in orderedInstallments)
        {
            var paymentTypeId = inst.IsAdvance
                ? "RECEIPT_ADVANCE_PAYMENT"
                : "RECEIPT_DUE_INSTALLMENT";

            var comments = inst.IsAdvance
                ? $"Advance payment ({inst.InstallmentNumber}) - SR {sr.SalesRequestId}"
                : $"Installment {inst.InstallmentNumber} - Due {inst.DueDate:yyyy-MM-dd} - SR {sr.SalesRequestId}";

            paymentsToCreate.Add(new CreatePaymentParam
            {
                PartyIdFrom = sr.FromPartyId!,           // Customer
                PartyIdTo = companyPartyId,              // Company
                Amount = inst.Amount,
                EffectiveDate = inst.DueDate,            // Use exact due date from plan
                PaymentTypeId = paymentTypeId,
                StatusId = "PMNT_NOT_PAID",              // Unpaid by default
                Comments = comments,
                SalesRequestId = sr.SalesRequestId,
                PaymentMethodId = null,
                PaymentMethodTypeId = null
            });
        }

        // 6. Create all payments
        foreach (var param in paymentsToCreate)
        {
            var payment = await _paymentHelperService.CreatePayment(param);
            if (payment == null)
            {
                await transaction.RollbackAsync(ct);
                return Result<CreateSalesRequest.SalesRequestResponseDto>.Failure(
                    "Failed to create one or more payments during approval");
            }
        }

        // 7. Load apartment for accounting description
        var apartment = await CreateSalesRequest.Handler.GetApartmentLovProjection(_context, sr.ProductId!, ct)
            ?? new CreateSalesRequest.ApartmentLovProjection { ApartmentName = "Unknown Apartment" };

        // 8. Create accounting transaction (full sale amount)
        var acctgTransParams = new CreateAcctgTransParams
        {
            AcctgTransTypeId = "APARTMENT_SALE",
            TransactionDate = sr.SaleDate ?? DateTime.UtcNow.Date,
            IsPosted = "Y",
            Description = $"Apartment Sale - SR {sr.SalesRequestId} - {apartment.ApartmentName}",
            GlFiscalTypeId = "ACTUAL",
            SalesRequestId = sr.SalesRequestId
        };

        var acctgTransId = await _acctgTransService.CreateAcctgTrans(acctgTransParams);

        var stamp = DateTime.UtcNow;
        var seq = 0;

        // Debit: Accounts Receivable
        await _acctgTransService.CreateAcctgTransEntry(new AcctgTransEntry
        {
            AcctgTransId = acctgTransId,
            AcctgTransEntrySeqId = (++seq).ToString().PadLeft(3, '0'),
            GlAccountId = "121100", // AR - Customers
            DebitCreditFlag = "D",
            AcctgTransEntryTypeId = "_NA_",
            Amount = sr.TotalPrice ?? 0m,
            ReconcileStatusId = "AES_NOT_RECONCILED",
            Description = $"Apartment sale receivable - {apartment.ApartmentName}",
            OrganizationPartyId = companyPartyId,
            CreatedStamp = stamp,
            LastUpdatedStamp = stamp
        });

        // Credit: Revenue
        await _acctgTransService.CreateAcctgTransEntry(new AcctgTransEntry
        {
            AcctgTransId = acctgTransId,
            AcctgTransEntrySeqId = (++seq).ToString().PadLeft(3, '0'),
            GlAccountId = "250120", // Revenue - Apartment Sales
            DebitCreditFlag = "C",
            AcctgTransEntryTypeId = "_NA_",
            Amount = sr.TotalPrice ?? 0m,
            ReconcileStatusId = "AES_NOT_RECONCILED",
            Description = $"Apartment sale revenue - {apartment.ApartmentName}",
            OrganizationPartyId = companyPartyId,
            CreatedStamp = stamp,
            LastUpdatedStamp = stamp
        });

        // 9. Save changes
        var saved = await _context.SaveChangesAsync(ct) > 0;
        if (!saved)
        {
            await transaction.RollbackAsync(ct);
            return Result<CreateSalesRequest.SalesRequestResponseDto>.Failure("Failed to approve sales request");
        }

        await transaction.CommitAsync(ct);

        // 10. Build and return response DTO
        var response = await BuildResponseDto(sr, ct);
        return Result<CreateSalesRequest.SalesRequestResponseDto>.Success(response);
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync(ct);
        return Result<CreateSalesRequest.SalesRequestResponseDto>.Failure(
            $"Failed to approve sales request: {ex.Message}");
    }
}