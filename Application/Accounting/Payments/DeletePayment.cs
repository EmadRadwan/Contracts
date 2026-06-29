using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Microsoft.Extensions.Logging;

namespace Application.Accounting.Payments;

public class DeletePayment
{
    public class Command : IRequest<Result<Unit>>
    {
        public string PaymentId { get; set; } = string.Empty;
    }

    public class Handler : IRequestHandler<Command, Result<Unit>>
    {
        private readonly DataContext _context;
        private readonly ILogger<Handler> _logger;

        public Handler(DataContext context, ILogger<Handler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var payment = await _context.Payments
                    .FirstOrDefaultAsync(p => p.PaymentId == request.PaymentId, cancellationToken);

                if (payment == null)
                {
                    return Result<Unit>.Failure($"Payment with ID {request.PaymentId} not found.");
                }

                if (!string.IsNullOrWhiteSpace(payment.SalesRequestId))
                {
                    return Result<Unit>.Failure(
                        $"لا يمكن حذف الدفعة لأنها مرتبطة بطلب مبيعات رقم: {payment.SalesRequestId}");
                }

                // Prevent deletion if payment is linked to an employee advance — must be managed from the advance screen
                var linkedAdvance = await _context.EmployeeAdvances
                    .Where(a => a.PaymentId == request.PaymentId)
                    .Select(a => a.AdvanceId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (linkedAdvance != null)
                {
                    return Result<Unit>.Failure(
                        $"لا يمكن حذف الدفعة لأنها مرتبطة بسلفة موظف رقم: {linkedAdvance}. يرجى حذفها من خلال شاشة سلف الموظفين.");
                }


                // Rule 2: Prevent deletion if payment is linked to a Project Certificate via PaymentPreference → Order → WorkEffort
                var isLinkedToProjectCertificate = await _context.Payments
                    .Where(p => p.PaymentId == request.PaymentId)
                    .AnyAsync(p => p.PaymentPreferenceId != null &&
                                   _context.OrderPaymentPreferences
                                       .Where(opp =>
                                           opp.OrderPaymentPreferenceId == p.PaymentPreferenceId && opp.OrderId != null)
                                       .Any(opp =>
                                           _context.WorkEfforts
                                               .Any(we => we.RelatedOrderId == opp.OrderId &&
                                                          we.WorkEffortTypeId == "PROJECT_CERTIFICATE")
                                       ), cancellationToken);

                if (isLinkedToProjectCertificate)
                {
                    // Optional: fetch certificate details for better message
                    var certificateInfo = await _context.OrderPaymentPreferences
                        .Where(opp => opp.OrderPaymentPreferenceId == payment.PaymentPreferenceId)
                        .SelectMany(opp => _context.WorkEfforts
                            .Where(we =>
                                we.RelatedOrderId == opp.OrderId && we.WorkEffortTypeId == "PROJECT_CERTIFICATE")
                            .Select(we => new { we.CertificateNumber, we.ProjectName }))
                        .FirstOrDefaultAsync(cancellationToken);

                    var certNum = certificateInfo?.CertificateNumber ?? "غير محدد";
                    var projName = string.IsNullOrWhiteSpace(certificateInfo?.ProjectName)
                        ? ""
                        : $" ({certificateInfo.ProjectName})";

                    return Result<Unit>.Failure(
                        $"لا يمكن حذف الدفعة لأنها مرتبطة بشهادة مشروع رقم: {certNum}{projName}");
                }

                // 4. Rule 3: NEW - Payment is applied to one or more Invoices
                var appliedInvoices = await (from pa in _context.PaymentApplications
                        join inv in _context.Invoices on pa.InvoiceId equals inv.InvoiceId
                        where pa.PaymentId == request.PaymentId
                        select inv.InvoiceId)
                    .ToListAsync(cancellationToken);

                if (appliedInvoices.Any())
                {
                    var invoiceList = string.Join("، ", appliedInvoices);
                    var message = appliedInvoices.Count == 1
                        ? $"لا يمكن حذف الدفعة لأنها مسجلة على فاتورة رقم: {invoiceList}"
                        : $"لا يمكن حذف الدفعة لأنها مسجلة على فواتير أرقام: {invoiceList}";

                    return Result<Unit>.Failure(message);
                }

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
                _logger.LogError(ex, "Error during payment deletion {PaymentId}", request.PaymentId);
                return Result<Unit>.Failure("حدث خطأ أثناء حذف الدفعة: " + ex.Message);
            }
        }
    }
}