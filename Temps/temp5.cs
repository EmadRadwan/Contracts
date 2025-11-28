public async Task<Results<IssueMaterialsForCertificateResult>> Handle(Command request, CancellationToken cancellationToken)
{
    // REFACTOR: Reuse existing transaction if present (e.g. called from ReceiveInventory)
    // Why: Enables atomic end-to-end flow: Receive → Create Certificate → Issue Materials
    await using var transaction = _context.Database.CurrentTransaction == null
        ? await _context.Database.BeginTransactionAsync(cancellationToken)
        : null;

    var ownsTransaction = transaction != null;

    try
    {
        var result = await _projectService.IssueMaterialsForCertificate(request.WorkEffortId);

        if (!result.IsSuccess)
        {
            if (ownsTransaction) await transaction!.RollbackAsync(cancellationToken);
            return Results<IssueMaterialsForCertificateResult>.Failure(result.ErrorMessage, result.ErrorCode);
        }

        var workEffort = await _context.WorkEfforts
            .FirstOrDefaultAsync(we => we.WorkEffortId == request.WorkEffortId && we.WorkEffortTypeId == "PROJECT_CERTIFICATE", cancellationToken);

        if (workEffort != null)
        {
            workEffort.CurrentStatusId = "WEPR_APPROVED";
            workEffort.LastStatusUpdate = DateTime.UtcNow;
            _context.WorkEfforts.Update(workEffort);
            _logger.LogInformation("Updated WorkEffort {WorkEffortId} status to WEPR_APPROVED", request.WorkEffortId);
        }

        var saveResult = await _context.SaveChangesAsync(cancellationToken);

        if (ownsTransaction)
            await transaction!.CommitAsync(cancellationToken);

        return Results<IssueMaterialsForCertificateResult>.Success(result.Value);
    }
    catch (Exception ex)
    {
        if (ownsTransaction && transaction != null)
            await transaction.RollbackAsync(cancellationToken);

        _logger.LogError(ex, "Error issuing materials for certificate WorkEffortId: {WorkEffortId}", request.WorkEffortId);
        return Results<IssueMaterialsForCertificateResult>.Failure(
            ex.Message ?? "An unexpected error occurred while issuing materials.");
    }
}