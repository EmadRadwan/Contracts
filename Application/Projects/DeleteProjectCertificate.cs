using Application.Accounting.Services;
using Application.Interfaces;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Projects
{
    public class DeleteProjectCertificate
    {
        public class Command : IRequest<Result<Unit>>
        {
            public string WorkEffortId { get; set; } = string.Empty;
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.WorkEffortId)
                    .NotEmpty().WithMessage("Work Effort ID is required");
            }
        }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            private readonly DataContext _context;
        private readonly ILedgerHistoryService _ledgerHistory;
        private readonly IAccountingPeriodGuard _periodGuard;
            private readonly IUserAccessor _userAccessor;

            public Handler(DataContext context, IUserAccessor userAccessor, IAccountingPeriodGuard periodGuard, ILedgerHistoryService ledgerHistory)
            {
                _context = context;
            _ledgerHistory = ledgerHistory;
            _periodGuard = periodGuard;
                _userAccessor = userAccessor;
            }

            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                _context.Database.SetCommandTimeout(300);
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

                try
                {
                    var certificateHeader = await _context.WorkEfforts
                        .Include(we => we.CurrentStatus)
                        .FirstOrDefaultAsync(we => we.WorkEffortId == request.WorkEffortId, cancellationToken);
                    await _periodGuard.EnsureOpenForWorkEffortsAsync(new[] { request.WorkEffortId }, cancellationToken);

                    if (certificateHeader == null)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return Result<Unit>.Failure("Certificate not found");
                    }


                    var relatedOrderId = certificateHeader.RelatedOrderId;

                    // Step 3 (auditor soft-delete requirement): an approved certificate is reset first,
                    // which reverses its postings; one that ever reached the ledger is then kept and
                    // marked cancelled rather than removed. Only a never-posted draft is deleted.
                    if (certificateHeader.CurrentStatusId == "WEPR_CANCELLED")
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return Result<Unit>.Failure("الشهادة ملغاة بالفعل. (Certificate is already cancelled.)");
                    }
                    if (certificateHeader.CurrentStatusId != "WEPR_CREATED")
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return Result<Unit>.Failure(
                            "لا يمكن حذف شهادة معتمدة. أعد تعيين الشهادة أولاً (تُعكس قيودها) ثم احذفها. " +
                            "(Reset the certificate first; approved certificates are not deleted.)");
                    }

                    var history = await _ledgerHistory.ForWorkEffortAsync(request.WorkEffortId, cancellationToken);
                    if (!history.HasHistory && !string.IsNullOrEmpty(relatedOrderId))
                    {
                        var orderPaymentIds = await (from opp in _context.OrderPaymentPreferences
                                join pm in _context.Payments on opp.OrderPaymentPreferenceId equals pm.PaymentPreferenceId
                                where opp.OrderId == relatedOrderId
                                select pm.PaymentId).ToListAsync(cancellationToken);
                        var orderInvoiceIds = await _context.OrderItemBillings
                            .Where(oib => oib.OrderId == relatedOrderId && oib.InvoiceId != null)
                            .Select(oib => oib.InvoiceId!)
                            .Distinct()
                            .ToListAsync(cancellationToken);
                        var viaPayments = await _ledgerHistory.ForPaymentsAsync(orderPaymentIds, cancellationToken);
                        var viaInvoices = await _ledgerHistory.ForInvoicesAsync(orderInvoiceIds, cancellationToken);
                        history.AcctgTransCount += viaPayments.AcctgTransCount + viaInvoices.AcctgTransCount;
                    }

                    if (history.HasHistory)
                    {
                        // Evidence (originals and their reversals) points at this certificate: keep it.
                        var stamp = DateTime.UtcNow;
                        certificateHeader.CurrentStatusId = "WEPR_CANCELLED";
                        certificateHeader.LastStatusUpdate = stamp;
                        certificateHeader.LastUpdatedStamp = stamp;
                        var cancelledItems = await _context.WorkEfforts
                            .Where(we => we.WorkEffortParentId == request.WorkEffortId)
                            .ToListAsync(cancellationToken);
                        foreach (var item in cancelledItems)
                        {
                            item.CurrentStatusId = "WEPR_CANCELLED";
                            item.LastUpdatedStamp = stamp;
                        }
                        if (!string.IsNullOrEmpty(relatedOrderId))
                        {
                            var order = await _context.OrderHeaders
                                .FirstOrDefaultAsync(oh => oh.OrderId == relatedOrderId, cancellationToken);
                            if (order != null) order.StatusId = "ORDER_CANCELLED";
                        }
                        await _context.SaveChangesAsync(cancellationToken);
                        await transaction.CommitAsync(cancellationToken);
                        return Result<Unit>.Success(Unit.Value);
                    }

                    certificateHeader.RelatedOrderId = null;
                    await _context.SaveChangesAsync(cancellationToken);

                    // 1. Delete child certificate items
                    var childItems = await _context.WorkEfforts
                        .Where(we => we.WorkEffortParentId == request.WorkEffortId)
                        .ToListAsync(cancellationToken);

                    _context.WorkEfforts.RemoveRange(childItems);

                    // 2. If there is a related order → delete it and all dependent rows first
                    if (!string.IsNullOrEmpty(relatedOrderId))
                    {
                        // Delete dependent tables in correct order (children first)
                        await _context.Set<OrderItemShipGroupAssoc>()
                            .Where(x => x.OrderId == relatedOrderId)
                            .ExecuteDeleteAsync(cancellationToken);
                        
                        await _context.OrderItemShipGroups
                            .Where(x => x.OrderId == relatedOrderId)
                            .ExecuteDeleteAsync(cancellationToken);
                        
                        await _context.Set<OrderItemBilling>()
                            .Where(x => x.OrderId == relatedOrderId)
                            .ExecuteDeleteAsync(cancellationToken);

                        await _context.OrderItems
                            .Where(x => x.OrderId == relatedOrderId)
                            .ExecuteDeleteAsync(cancellationToken);

                        await _context.OrderAdjustments
                            .Where(x => x.OrderId == relatedOrderId)
                            .ExecuteDeleteAsync(cancellationToken);

                        await _context.OrderStatuses
                            .Where(x => x.OrderId == relatedOrderId)
                            .ExecuteDeleteAsync(cancellationToken);

                        await _context.OrderRoles
                            .Where(x => x.OrderId == relatedOrderId)
                            .ExecuteDeleteAsync(cancellationToken);
                        
                        
                        
                        var paymentPrefIds = await _context.OrderPaymentPreferences
                            .Where(p => p.OrderId == relatedOrderId)
                            .Select(p => p.OrderPaymentPreferenceId)
                            .ToListAsync(cancellationToken);

                        if (paymentPrefIds.Any())
                        {
                            var certificatePaymentIds = await _context.Payments
                                .Where(p => paymentPrefIds.Contains(p.PaymentPreferenceId))
                                .Select(p => p.PaymentId)
                                .ToListAsync(cancellationToken);
                            await _periodGuard.EnsureOpenForPaymentsAsync(certificatePaymentIds, cancellationToken);

                            await _context.Payments
                                .Where(p => paymentPrefIds.Contains(p.PaymentPreferenceId))
                                .ExecuteDeleteAsync(cancellationToken);
                        }

                        await _context.OrderPaymentPreferences
                            .Where(p => p.OrderId == relatedOrderId)
                            .ExecuteDeleteAsync(cancellationToken);


                        // ← Delete the order header BEFORE touching the WorkEffort
                        await _context.OrderHeaders
                            .Where(h => h.OrderId == relatedOrderId)
                            .ExecuteDeleteAsync(cancellationToken);
                    }

                    // 3. Now it's safe to delete the certificate header
                    // (no FK violation because RELATED_ORDER_ID either was null or now points to a non-existing row — but MySQL won't complain because the row is gone)
                    _context.WorkEfforts.Remove(certificateHeader);

                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    return Result<Unit>.Success(Unit.Value);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result<Unit>.Failure($"Failed to delete certificate: {ex.Message}");
                }
            }
        }
    }
}