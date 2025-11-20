// 3. NOW create new PO — change tracker is clean!
var poResult = await _orderService.CreatePurchaseOrder(orderDto);
_context.ChangeTracker.Clear();

if (poResult == null)
{
    await transaction.RollbackAsync(cancellationToken);
    return Result<ProjectCertificateDto>.Failure("Failed to create purchase order");
}

newOrderId = poResult.OrderId;

// APPROVE
// … (approval code unchanged) …

// CRITICAL FIX: set and persist the RelatedOrderId
workEffortQuery.RelatedOrderId = newOrderId;

await _context.SaveChangesAsync(cancellationToken);   // ← ADD THIS

// Final commit
await transaction.CommitAsync(cancellationToken);