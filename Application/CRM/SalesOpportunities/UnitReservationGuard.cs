using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.CRM.SalesOpportunities;

/// <summary>
/// A unit can only be won once. Two code paths mark a deal as won -
/// UpdateSalesOpportunity (stage moved to Closed Won, including the board's
/// drag-and-drop) and CreateSalesOpportunityAction (the DONE_DEAL action) - so
/// the rule lives here rather than being written twice and drifting.
///
/// Winning no longer touches Product.ApartmentStatusId: APARTMENT_RESERVED was
/// retired in Sep 2026 and the CRM never had a release path for it anyway. The
/// sale itself (and the unit lock) is owned by the Sales Request module.
/// </summary>
public static class UnitReservationGuard
{
    public const string ClosedWonStageId = "SOSTG_CLOSED_WON";
    public const string SoldStatusId = "APARTMENT_SOLD";

    /// <summary>
    /// Returns an error message when <paramref name="claimingOpportunityId"/> may not
    /// win <paramref name="productId"/>, or null when it is free to take.
    ///
    /// The conflict is detected on the STAGE rather than the IsWon flag: flag
    /// maintenance was only added in Aug 2026, so rows closed before then still
    /// carry IsWon = false and would slip past a flag-based check.
    /// </summary>
    public static async Task<string?> CheckAsync(
        DataContext context,
        string? productId,
        string claimingOpportunityId,
        CancellationToken ct)
    {
        // An opportunity with no unit attached reserves nothing.
        if (string.IsNullOrWhiteSpace(productId))
            return null;

        var conflict = await context.SalesOpportunities
            .Where(o => o.ProductId == productId
                     && o.SalesOpportunityId != claimingOpportunityId
                     && o.OpportunityStageId == ClosedWonStageId)
            .Select(o => new
            {
                o.SalesOpportunityId,
                o.OpportunityName,
                LeadName = context.SalesOpportunityRoles
                    .Where(r => r.SalesOpportunityId == o.SalesOpportunityId
                             && r.RoleTypeId == "LEAD")
                    .Join(context.Parties, r => r.PartyId, p => p.PartyId, (r, p) => p.Description)
                    .FirstOrDefault()
            })
            .FirstOrDefaultAsync(ct);

        if (conflict != null)
        {
            var who = string.IsNullOrWhiteSpace(conflict.LeadName)
                ? $"opportunity {conflict.SalesOpportunityId}"
                : $"{conflict.LeadName} (opportunity {conflict.SalesOpportunityId})";

            return $"Unit '{productId}' has already been won by {who}. " +
                   "Reopen or cancel that opportunity before winning this one.";
        }

        // A sold unit is gone for good - the sale lives outside the CRM, so no
        // opportunity may claim it, whether or not a won opportunity is on record.
        // A unit with a pending sales request is equally spoken for.
        var unit = await context.Products
            .Where(p => p.ProductId == productId)
            .Select(p => new { p.ApartmentStatusId, p.ReservedBySalesRequestId })
            .FirstOrDefaultAsync(ct);

        if (unit?.ApartmentStatusId == SoldStatusId)
            return $"Unit '{productId}' is already sold and cannot be won again.";

        var holder = unit?.ReservedBySalesRequestId;
        if (!string.IsNullOrWhiteSpace(holder))
        {
            var pending = await context.SalesRequests
                .AnyAsync(s => s.SalesRequestId == holder
                            && s.StatusId != "SALES_REQUEST_CANCELLED", ct);
            if (pending)
                return $"Unit '{productId}' already has an open sales request " +
                       $"({holder}) and cannot be won.";
        }

        return null;
    }
}
