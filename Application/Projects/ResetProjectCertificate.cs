using Application.Core;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Projects;

public class ResetProjectCertificate
{
    public class Command : IRequest<Result<Unit>>
    {
        public string WorkEffortId { get; set; } = string.Empty;
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.WorkEffortId).NotEmpty().WithMessage("Work Effort ID is required");
        }
    }

    public class Handler : IRequestHandler<Command, Result<Unit>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            _context.Database.SetCommandTimeout(300);
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var workEffort = await _context.WorkEfforts
                    .FirstOrDefaultAsync(we => we.WorkEffortId == request.WorkEffortId, cancellationToken);

                if (workEffort == null)
                {
                    return Result<Unit>.Failure("Certificate not found");
                }

                if (workEffort.CurrentStatusId == "WEPR_CREATED")
                {
                    return Result<Unit>.Failure("Certificate is already in Created status");
                }

                var category = workEffort.CertificateCategory;
                var relatedOrderId = workEffort.RelatedOrderId;

                // 1. Revert WorkEffort status
                workEffort.CurrentStatusId = "WEPR_CREATED";
                workEffort.LastStatusUpdate = DateTime.UtcNow;

                // 2. Handle Related Order (Purchase Order)
                if (!string.IsNullOrEmpty(relatedOrderId))
                {
                    var order = await _context.OrderHeaders
                        .FirstOrDefaultAsync(oh => oh.OrderId == relatedOrderId, cancellationToken);

                    if (order != null)
                    {
                        order.StatusId = "ORDER_CREATED";

                        // Remove status history entries except the initial one (optional, but cleaner)
                        var statusesToRemove = await _context.OrderStatuses
                            .Where(os => os.OrderId == relatedOrderId && os.StatusId != "ORDER_CREATED")
                            .ToListAsync(cancellationToken);
                        _context.OrderStatuses.RemoveRange(statusesToRemove);
                    }

                    // 2b. Revert Order Roles
                    /*var orderRoles = await _context.OrderRoles
                        .Where(or => or.OrderId == relatedOrderId)
                        .ToListAsync(cancellationToken);
                    _context.OrderRoles.RemoveRange(orderRoles);*/

                    // 2c. Revert Order Item Ship Group Assocs and Ship Groups
                    /*var oisga = await _context.Set<OrderItemShipGroupAssoc>()
                        .Where(x => x.OrderId == relatedOrderId)
                        .ToListAsync(cancellationToken);
                    _context.RemoveRange(oisga);

                    var oisg = await _context.OrderItemShipGroups
                        .Where(x => x.OrderId == relatedOrderId)
                        .ToListAsync(cancellationToken);
                    _context.OrderItemShipGroups.RemoveRange(oisg);*/

                    // 2d. Revert Payments and Payment Preferences associated with the order
                    var paymentPrefs = await _context.OrderPaymentPreferences
                        .Where(opp => opp.OrderId == relatedOrderId)
                        .ToListAsync(cancellationToken);

                    var oppIds = paymentPrefs.Select(opp => opp.OrderPaymentPreferenceId).ToList();

                    var payments = await _context.Payments
                        .Where(p => oppIds.Contains(p.PaymentPreferenceId))
                        .ToListAsync(cancellationToken);

                    var paymentIds = payments.Select(p => p.PaymentId).ToList();

                    // Cleanup Payment related artifacts (Accounting Transactions, etc. from ResetPayment.cs)
                    if (paymentIds.Any())
                    {
                        var paymentTransEntries = await _context.AcctgTransEntries
                            .Where(ate => _context.AcctgTrans.Any(at =>
                                at.AcctgTransId == ate.AcctgTransId && paymentIds.Contains(at.PaymentId)))
                            .ToListAsync(cancellationToken);
                        _context.AcctgTransEntries.RemoveRange(paymentTransEntries);

                        var paymentTrans = await _context.AcctgTrans.Where(at => paymentIds.Contains(at.PaymentId))
                            .ToListAsync(cancellationToken);
                        _context.AcctgTrans.RemoveRange(paymentTrans);
                    }

                    var paymentApplications = await _context.PaymentApplications
                        .Where(pa => paymentIds.Contains(pa.PaymentId))
                        .ToListAsync(cancellationToken);

                    _context.PaymentApplications.RemoveRange(paymentApplications);
                    _context.Payments.RemoveRange(payments);
                    _context.OrderPaymentPreferences.RemoveRange(paymentPrefs);
                }

                // 3. Category-specific reversals
                if (category is "SUPPLY_PROCUREMENT_CERTIFICATE" or "WORKMANSHIP_CONTRACTING_CERTIFICATE")
                {
                    // Handle linked COMPANY_SUPPLY_SALE_CERTIFICATE (DeliverToSite side effect)
                    if (category == "SUPPLY_PROCUREMENT_CERTIFICATE")
                    {
                        var linkedSaleCertificates = await _context.WorkEfforts
                            .Where(we => we.CertificateCategory == "COMPANY_SUPPLY_SALE_CERTIFICATE" &&
                                         we.Description.Contains(workEffort.CertificateNumber))
                            .ToListAsync(cancellationToken);

                        foreach (var linkedCert in linkedSaleCertificates)
                        {
                            // 1. Identify all InventoryItemDetails linked to this certificate
                            var details = await _context.InventoryItemDetails
                                .Where(iid => iid.WorkEffortId == linkedCert.WorkEffortId)
                                .ToListAsync(cancellationToken);

                            foreach (var detail in details)
                            {
                                var inventoryItem = await _context.InventoryItems
                                    .FirstOrDefaultAsync(ii => ii.InventoryItemId == detail.InventoryItemId,
                                        cancellationToken);

                                if (inventoryItem != null)
                                {
                                    // Add back the issued quantity (diff is negative for issuances)
                                    inventoryItem.QuantityOnHandTotal -= detail.QuantityOnHandDiff ?? 0;
                                    inventoryItem.AvailableToPromiseTotal -= detail.AvailableToPromiseDiff ?? 0;
                                }
                            }

                            // 4. Remove Accounting Transactions for the linked certificate
                            var trans = await _context.AcctgTrans
                                .Where(at => at.WorkEffortId == linkedCert.WorkEffortId).ToListAsync(cancellationToken);
                            foreach (var tran in trans)
                            {
                                var entries = await _context.AcctgTransEntries
                                    .Where(e => e.AcctgTransId == tran.AcctgTransId).ToListAsync(cancellationToken);
                                _context.AcctgTransEntries.RemoveRange(entries);
                            }

                            _context.AcctgTrans.RemoveRange(trans);

                            _context.InventoryItemDetails.RemoveRange(details);

                            // 5. Delete the linked certificate items and the certificate itself
                            var linkedCertItems = await _context.WorkEfforts
                                .Where(we => we.WorkEffortParentId == linkedCert.WorkEffortId)
                                .ToListAsync(cancellationToken);

                            _context.WorkEfforts.RemoveRange(linkedCertItems);
                            _context.WorkEfforts.Remove(linkedCert);
                        }
                    }

                    // Revert Inventory Receipts (Only for SUPPLY_PROCUREMENT_CERTIFICATE)
                    if (category == "SUPPLY_PROCUREMENT_CERTIFICATE")
                    {
                        var receipts = await _context.ShipmentReceipts
                            .Where(sr => sr.OrderId == relatedOrderId)
                            .ToListAsync(cancellationToken);

                        foreach (var receipt in receipts)
                        {
                            if (!string.IsNullOrEmpty(receipt.InventoryItemId))
                            {
                                var inventoryItem = await _context.InventoryItems
                                    .Include(ii => ii.InventoryItemDetails)
                                    .FirstOrDefaultAsync(ii => ii.InventoryItemId == receipt.InventoryItemId,
                                        cancellationToken);

                                if (inventoryItem != null)
                                {
                                    // Remove details linked to this receipt
                                    var details = inventoryItem.InventoryItemDetails
                                        .Where(iid => iid.InventoryItemId == inventoryItem.InventoryItemId)
                                        .ToList();

                                    // 3b. Remove Billing and Accounting for receipt
                                    var oibs = await _context.OrderItemBillings
                                        .Where(o => o.ShipmentReceiptId == receipt.ReceiptId)
                                        .ToListAsync(cancellationToken);
                                    var invoiceIds = oibs.Select(o => o.InvoiceId).Distinct().ToList();

                                    _context.OrderItemBillings.RemoveRange(oibs);

                                    if (invoiceIds.Any())
                                    {
                                        await CleanupInvoices(invoiceIds, cancellationToken);
                                    }

                                    var trans = await _context.AcctgTrans.Where(at => at.ReceiptId == receipt.ReceiptId)
                                        .ToListAsync(cancellationToken);
                                    foreach (var tran in trans)
                                    {
                                        var entries = await _context.AcctgTransEntries
                                            .Where(e => e.AcctgTransId == tran.AcctgTransId)
                                            .ToListAsync(cancellationToken);
                                        _context.AcctgTransEntries.RemoveRange(entries);
                                    }

                                    _context.AcctgTrans.RemoveRange(trans);

                                    _context.InventoryItemDetails.RemoveRange(details);

                                    // If this was the only receipt for this item, remove the item
                                    if (!inventoryItem.InventoryItemDetails.Any(d => !details.Contains(d)))
                                    {
                                        _context.InventoryItems.Remove(inventoryItem);
                                    }
                                    else
                                    {
                                        // Otherwise adjust totals (though for new items created via receipt, they are usually 1-to-1)
                                        foreach (var detail in details)
                                        {
                                            inventoryItem.QuantityOnHandTotal -= detail.QuantityOnHandDiff ?? 0;
                                            inventoryItem.AvailableToPromiseTotal -= detail.AvailableToPromiseDiff ?? 0;
                                        }
                                    }
                                }
                            }
                        }

                        _context.ShipmentReceipts.RemoveRange(receipts);
                    }

                    // For WORKMANSHIP_CONTRACTING_CERTIFICATE, there might be invoices directly linked to the order
                    if (category == "WORKMANSHIP_CONTRACTING_CERTIFICATE" && !string.IsNullOrEmpty(relatedOrderId))
                    {
                        var orderBillings = await _context.OrderItemBillings
                            .Where(oib => oib.OrderId == relatedOrderId)
                            .ToListAsync(cancellationToken);

                        var invoiceIds = orderBillings.Select(oib => oib.InvoiceId).Distinct().ToList();

                        _context.OrderItemBillings.RemoveRange(orderBillings);

                        if (invoiceIds.Any())
                        {
                            await CleanupInvoices(invoiceIds, cancellationToken);
                        }
                    }

                    // Revert Shipments
                    if (!string.IsNullOrEmpty(relatedOrderId))
                    {
                        var orderShipments = await _context.OrderShipments
                            .Where(os => os.OrderId == relatedOrderId)
                            .ToListAsync(cancellationToken);

                        var shipmentIds = orderShipments.Select(os => os.ShipmentId).Distinct().ToList();

                        _context.OrderShipments.RemoveRange(orderShipments);

                        var shipmentItems = await _context.ShipmentItems
                            .Where(si => shipmentIds.Contains(si.ShipmentId))
                            .ToListAsync(cancellationToken);

                        // 3c. Final cleanup for shipments (Billing/Accounting that might have been missed)
                        var sibs = await _context.ShipmentItemBillings.Where(s => shipmentIds.Contains(s.ShipmentId))
                            .ToListAsync(cancellationToken);
                        _context.ShipmentItemBillings.RemoveRange(sibs);

                        var shipTrans = await _context.AcctgTrans.Where(at => shipmentIds.Contains(at.ShipmentId))
                            .ToListAsync(cancellationToken);
                        foreach (var tran in shipTrans)
                        {
                            var entries = await _context.AcctgTransEntries
                                .Where(e => e.AcctgTransId == tran.AcctgTransId).ToListAsync(cancellationToken);
                            _context.AcctgTransEntries.RemoveRange(entries);
                        }

                        _context.AcctgTrans.RemoveRange(shipTrans);

                        var shipmentStatuses = await _context.ShipmentStatuses
                            .Where(ss => shipmentIds.Contains(ss.ShipmentId)).ToListAsync(cancellationToken);
                        _context.ShipmentStatuses.RemoveRange(shipmentStatuses);

                        var rs = await _context.ShipmentRouteSegments.Where(r => shipmentIds.Contains(r.ShipmentId))
                            .ToListAsync(cancellationToken);
                        var rsIds = rs.Select(r => r.ShipmentRouteSegmentId).ToList();
                        var sprs = await _context.ShipmentPackageRouteSegs
                            .Where(s => shipmentIds.Contains(s.ShipmentId) && rsIds.Contains(s.ShipmentRouteSegmentId))
                            .ToListAsync(cancellationToken);
                        _context.ShipmentPackageRouteSegs.RemoveRange(sprs);
                        _context.ShipmentRouteSegments.RemoveRange(rs);

                        _context.ShipmentItems.RemoveRange(shipmentItems);

                        var shipments = await _context.Shipments
                            .Where(s => shipmentIds.Contains(s.ShipmentId))
                            .ToListAsync(cancellationToken);
                        _context.Shipments.RemoveRange(shipments);
                    }
                }
                else if (category == "COMPANY_SUPPLY_SALE_CERTIFICATE")
                {
                    // 1. Identify all InventoryItemDetails linked to this certificate
                    var details = await _context.InventoryItemDetails
                        .Where(iid => iid.WorkEffortId == request.WorkEffortId)
                        .ToListAsync(cancellationToken);

                    foreach (var detail in details)
                    {
                        var inventoryItem = await _context.InventoryItems
                            .FirstOrDefaultAsync(ii => ii.InventoryItemId == detail.InventoryItemId,
                                cancellationToken);

                        if (inventoryItem != null)
                        {
                            // Add back the issued quantity (diff is negative for issuances)
                            inventoryItem.QuantityOnHandTotal -= detail.QuantityOnHandDiff ?? 0;
                            inventoryItem.AvailableToPromiseTotal -= detail.AvailableToPromiseDiff ?? 0;
                        }
                    }

                    // 2. Identify and remove ItemIssuances linked via details or RelatedOrderId
                    /*var issuanceIdsFromDetails = details.Where(d => !string.IsNullOrEmpty(d.ItemIssuanceId))
                        .Select(d => d.ItemIssuanceId!)
                        .Distinct()
                        .ToList();

                    var issuances = await _context.ItemIssuances
                        .Where(ii => issuanceIdsFromDetails.Contains(ii.ItemIssuanceId) ||
                                     (relatedOrderId != null && (ii.OrderId == relatedOrderId ||
                                                                 ii.ShipmentId != null &&
                                                                 _context.Shipments.Any(s =>
                                                                     s.ShipmentId == ii.ShipmentId &&
                                                                     s.PrimaryOrderId == relatedOrderId))))
                        .ToListAsync(cancellationToken);

                    foreach (var issuance in issuances)
                    {
                        // 3a. Remove Billing and Accounting for sale issuance
                        var oibs = await _context.OrderItemBillings
                            .Where(o => o.ItemIssuanceId == issuance.ItemIssuanceId).ToListAsync(cancellationToken);
                        var sibs = await _context.ShipmentItemBillings
                            .Where(s => s.ShipmentId == issuance.ShipmentId &&
                                        s.ShipmentItemSeqId == issuance.ShipmentItemSeqId)
                            .ToListAsync(cancellationToken);
                        var invoiceIds = oibs.Select(o => o.InvoiceId).Union(sibs.Select(s => s.InvoiceId)).Distinct()
                            .ToList();

                        _context.OrderItemBillings.RemoveRange(oibs);
                        _context.ShipmentItemBillings.RemoveRange(sibs);

                        if (invoiceIds.Any())
                        {
                            await CleanupInvoices(invoiceIds, cancellationToken);
                        }

                        // Remove Shipment artifacts if present
                        if (!string.IsNullOrEmpty(issuance.ShipmentId))
                        {
                            var shipmentStatuses = await _context.ShipmentStatuses
                                .Where(ss => ss.ShipmentId == issuance.ShipmentId).ToListAsync(cancellationToken);
                            _context.ShipmentStatuses.RemoveRange(shipmentStatuses);

                            var rs = await _context.ShipmentRouteSegments
                                .Where(r => r.ShipmentId == issuance.ShipmentId).ToListAsync(cancellationToken);
                            var rsIds = rs.Select(r => r.ShipmentRouteSegmentId).ToList();
                            var sprs = await _context.ShipmentPackageRouteSegs
                                .Where(s => s.ShipmentId == issuance.ShipmentId &&
                                            rsIds.Contains(s.ShipmentRouteSegmentId)).ToListAsync(cancellationToken);
                            _context.ShipmentPackageRouteSegs.RemoveRange(sprs);
                            _context.ShipmentRouteSegments.RemoveRange(rs);
                        }
                    }

                    _context.ItemIssuances.RemoveRange(issuances);
                    */

                    // 4. Remove Accounting Transactions for the certificate
                    var trans = await _context.AcctgTrans
                        .Where(at =>
                            at.WorkEffortId == request.WorkEffortId )
                        .ToListAsync(cancellationToken);
                    foreach (var tran in trans)
                    {
                        var entries = await _context.AcctgTransEntries.Where(e => e.AcctgTransId == tran.AcctgTransId)
                            .ToListAsync(cancellationToken);
                        _context.AcctgTransEntries.RemoveRange(entries);
                    }

                    _context.AcctgTrans.RemoveRange(trans);

                    _context.InventoryItemDetails.RemoveRange(details);
                }

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return Result<Unit>.Success(Unit.Value);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<Unit>.Failure($"Failed to reset certificate: {ex.Message}");
            }
        }

        private async Task CleanupInvoices(List<string> invoiceIds, CancellationToken cancellationToken)
        {
            var oabs = await _context.OrderAdjustmentBillings.Where(oab => invoiceIds.Contains(oab.InvoiceId))
                .ToListAsync(cancellationToken);
            _context.OrderAdjustmentBillings.RemoveRange(oabs);

            var invoiceRoles = await _context.InvoiceRoles.Where(ir => invoiceIds.Contains(ir.InvoiceId))
                .ToListAsync(cancellationToken);
            _context.InvoiceRoles.RemoveRange(invoiceRoles);

            var invoiceStatuses = await _context.InvoiceStatuses.Where(isat => invoiceIds.Contains(isat.InvoiceId))
                .ToListAsync(cancellationToken);
            _context.InvoiceStatuses.RemoveRange(invoiceStatuses);

            var pAs = await _context.PaymentApplications.Where(pa => invoiceIds.Contains(pa.InvoiceId))
                .ToListAsync(cancellationToken);
            _context.PaymentApplications.RemoveRange(pAs);

            var invAttrs = await _context.InvoiceAttributes.Where(ia => invoiceIds.Contains(ia.InvoiceId))
                .ToListAsync(cancellationToken);
            _context.InvoiceAttributes.RemoveRange(invAttrs);

            var invCMS = await _context.InvoiceContactMeches.Where(icm => invoiceIds.Contains(icm.InvoiceId))
                .ToListAsync(cancellationToken);
            _context.InvoiceContactMeches.RemoveRange(invCMS);

            var invContents = await _context.InvoiceContents.Where(ic => invoiceIds.Contains(ic.InvoiceId))
                .ToListAsync(cancellationToken);
            _context.InvoiceContents.RemoveRange(invContents);

            var invTerms = await _context.InvoiceTerms.Where(it => invoiceIds.Contains(it.InvoiceId))
                .ToListAsync(cancellationToken);
            var invTermIds = invTerms.Select(it => it.InvoiceTermId).ToList();
            var invTermAttrs = await _context.InvoiceTermAttributes.Where(ita => invTermIds.Contains(ita.InvoiceTermId))
                .ToListAsync(cancellationToken);
            _context.InvoiceTermAttributes.RemoveRange(invTermAttrs);
            _context.InvoiceTerms.RemoveRange(invTerms);

            var invoiceItems = await _context.InvoiceItems.Where(ii => invoiceIds.Contains(ii.InvoiceId))
                .ToListAsync(cancellationToken);
            var invItemAttrs = await _context.InvoiceItemAttributes.Where(iia => invoiceIds.Contains(iia.InvoiceId))
                .ToListAsync(cancellationToken);
            _context.InvoiceItemAttributes.RemoveRange(invItemAttrs);

            // From ResetInvoice.cs: Accounting Transactions for Invoice
            var invoiceTransEntries = await _context.AcctgTransEntries
                .Where(ate =>
                    _context.AcctgTrans.Any(at =>
                        at.AcctgTransId == ate.AcctgTransId && invoiceIds.Contains(at.InvoiceId)))
                .ToListAsync(cancellationToken);
            _context.AcctgTransEntries.RemoveRange(invoiceTransEntries);

            var invoiceTrans = await _context.AcctgTrans.Where(at => invoiceIds.Contains(at.InvoiceId))
                .ToListAsync(cancellationToken);
            _context.AcctgTrans.RemoveRange(invoiceTrans);

            _context.InvoiceItems.RemoveRange(invoiceItems);
            var invoices = await _context.Invoices.Where(i => invoiceIds.Contains(i.InvoiceId))
                .ToListAsync(cancellationToken);
            _context.Invoices.RemoveRange(invoices);
        }
    }
}