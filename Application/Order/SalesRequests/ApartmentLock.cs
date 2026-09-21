using Domain;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Order.SalesRequests;

/// <summary>
/// Who may put a sales request on a unit. Since Sep 2026 an apartment is only ever
/// APARTMENT_AVAILABLE or APARTMENT_SOLD — the intermediate APARTMENT_RESERVED status was
/// retired because four unrelated modules wrote it and none could tell who held the unit.
/// The hold on a unit with a pending request is Product.ReservedBySalesRequestId, and this
/// is the single place that reads it, shared by Create and Update so the two cannot drift.
/// </summary>
public static class ApartmentLock
{
    public const string AvailableStatusId = "APARTMENT_AVAILABLE";
    public const string SoldStatusId = "APARTMENT_SOLD";

    /// <summary>
    /// Returns an error message when <paramref name="claimingSalesRequestId"/> may not take
    /// <paramref name="apartment"/>, or null when it is free (or already held by that request).
    /// A dangling pointer to a cancelled or deleted request does not block.
    /// </summary>
    public static async Task<string?> CheckAsync(
        DataContext context,
        Product apartment,
        string claimingSalesRequestId,
        CancellationToken ct)
    {
        if (apartment.ApartmentStatusId == SoldStatusId)
            return $"الوحدة '{apartment.ProductId}' مباعة بالفعل. (Apartment is already sold.)";

        var holder = apartment.ReservedBySalesRequestId;
        if (string.IsNullOrWhiteSpace(holder) || holder == claimingSalesRequestId)
            return null;

        var holderIsLive = await context.SalesRequests
            .AnyAsync(s => s.SalesRequestId == holder && s.StatusId != "SALES_REQUEST_CANCELLED", ct);

        return holderIsLive
            ? $"الوحدة '{apartment.ProductId}' عليها طلب مبيعات قائم ({holder}). " +
              $"(Apartment already has an open sales request {holder}.)"
            : null;
    }
}
