public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
{
    using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

    try
    {
        // 1. Load payment
        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.PaymentId == request.PaymentId, cancellationToken);

        if (payment == null)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result<Unit>.Failure($"الدفعة برقم {request.PaymentId} غير موجودة.");
        }

        // 2. Blocking checks (unchanged)
        if (!string.IsNullOrWhiteSpace(payment.SalesRequestId))
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result<Unit>.Failure(
                $"لا يمكن حذف الدفعة لأنها مرتبطة بطلب مبيعات رقم: {payment.SalesRequestId}");
        }

        var hasProjectCertificate = await (from p in _context.Payments
                                           join opp in _context.OrderPaymentPreferences
                                               on p.PaymentPreferenceId equals opp.OrderPaymentPreferenceId into oppJoin
                                           from opp in oppJoin.DefaultIfEmpty()
                                           join we in _context.WorkEfforts
                                               on opp.OrderId equals we.RelatedOrderId into weJoin
                                           from we in weJoin.DefaultIfEmpty()
                                           where p.PaymentId == request.PaymentId
                                              && we.WorkEffortTypeId == "PROJECT_CERTIFICATE"
                                              && opp.OrderId != null
                                           select 1)
                                          .AnyAsync(cancellationToken);

        if (hasProjectCertificate)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result<Unit>.Failure("لا يمكن حذف الدفعة لأنها مرتبطة بشهادة مشروع.");
        }

        var appliedInvoices = await _context.PaymentApplications
            .Where(pa => pa.PaymentId == request.PaymentId)
            .Select(pa => pa.InvoiceId)
            .ToListAsync(cancellationToken);

        if (appliedInvoices.Any())
        {
            await transaction.RollbackAsync(cancellationToken);
            var invoiceList = string.Join("، ", appliedInvoices);
            var msg = appliedInvoices.Count == 1
                ? $"لا يمكن حذف الدفعة لأنها مسجلة على فاتورة رقم: {invoiceList}"
                : $"لا يمكن حذف الدفعة لأنها مسجلة على فواتير أرقام: {invoiceList}";
            return Result<Unit>.Failure(msg);
        }

        var blockedStatuses = new[] { "PMNT_SENT", "PMNT_RECEIVED", "PMNT_CONFIRMED" };
        if (blockedStatuses.Contains(payment.StatusId))
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result<Unit>.Failure(
                "لا يمكن حذف الدفعة لأن حالتها (مرسلة/مستلمة/مؤكدة) لا تسمح بالحذف.");
        }

        // 3. Cleanup related records using Select → RemoveRange → SaveChanges

        int cleanedAcctgTrans = 0;
        int cleanedAcctgEntries = 0;
        int cleanedFinTrans = 0;

        // Cleanup Accounting Transactions
        var relatedTransIds = await _context.AcctgTrans
            .Where(t => t.PaymentId == request.PaymentId)
            .Select(t => t.AcctgTransId)
            .ToListAsync(cancellationToken);

        if (relatedTransIds.Any())
        {
            // Delete AcctgTransEntries first
            var entriesToDelete = await _context.AcctgTransEntries
                .Where(e => relatedTransIds.Contains(e.AcctgTransId))
                .ToListAsync(cancellationToken);

            if (entriesToDelete.Any())
            {
                _context.AcctgTransEntries.RemoveRange(entriesToDelete);
                cleanedAcctgEntries = entriesToDelete.Count;
            }

            // Then delete AcctgTrans
            var transToDelete = await _context.AcctgTrans
                .Where(t => relatedTransIds.Contains(t.AcctgTransId))
                .ToListAsync(cancellationToken);

            if (transToDelete.Any())
            {
                _context.AcctgTrans.RemoveRange(transToDelete);
                cleanedAcctgTrans = transToDelete.Count;
            }

            // Save cleanup of accounting transactions
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Cleaned up {Entries} AcctgTransEntries and {Trans} AcctgTrans for payment {PaymentId}",
                cleanedAcctgEntries, cleanedAcctgTrans, request.PaymentId);
        }

        // Cleanup FinAccountTrans
        var finTransToDelete = await _context.FinAccountTrans
            .Where(f => f.PaymentId == request.PaymentId)
            .ToListAsync(cancellationToken);

        if (finTransToDelete.Any())
        {
            _context.FinAccountTrans.RemoveRange(finTransToDelete);
            cleanedFinTrans = finTransToDelete.Count;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Cleaned up {Count} FinAccountTrans records for payment {PaymentId}",
                cleanedFinTrans, request.PaymentId);
        }

        // 4. Finally delete the payment
        _context.Payments.Remove(payment);

        var finalSave = await _context.SaveChangesAsync(cancellationToken) > 0;

        if (!finalSave)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError("Failed to delete payment {PaymentId} after cleanup", request.PaymentId);
            return Result<Unit>.Failure("فشل في حفظ التغييرات أثناء حذف الدفعة.");
        }

        await transaction.CommitAsync(cancellationToken);

        _logger.LogInformation(
            "Successfully deleted payment {PaymentId}. Cleaned: {AcctgEntries} entries, {AcctgTrans} transactions, {FinTrans} fin trans.",
            request.PaymentId, cleanedAcctgEntries, cleanedAcctgTrans, cleanedFinTrans);

        return Result<Unit>.Success(Unit.Value);
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync(cancellationToken);
        _logger.LogError(ex, "Exception during deletion of payment {PaymentId}", request.PaymentId);
        return Result<Unit>.Failure("حدث خطأ غير متوقع أثناء حذف الدفعة. تم إلغاء جميع التغييرات.");
    }
}