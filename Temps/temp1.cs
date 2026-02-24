// ────────────────────────────────────────────────────────────────
//   Replicate OrderPaymentPreference if original had one
// ────────────────────────────────────────────────────────────────
if (!string.IsNullOrEmpty(original.PaymentPreferenceId))
{
    var originalPreference = await _context.OrderPaymentPreferences
        .AsNoTracking()
        .FirstOrDefaultAsync(opp => opp.OrderPaymentPreferenceId == original.PaymentPreferenceId, ct);

    if (originalPreference != null && !string.IsNullOrEmpty(originalPreference.OrderId))
    {
        var newPreference = new OrderPaymentPreference
        {
            // ID generation – adjust to match your actual strategy
            // Common patterns in OFBiz-style systems: sequential, GUID, or custom prefix
            OrderPaymentPreferenceId = Guid.NewGuid().ToString("N").ToUpperInvariant(),  
            // If using database-generated ID → leave null and let EF handle

            OrderId                  = originalPreference.OrderId,          // same PO / SO
            // NO PaymentId here – that's correct, table doesn't have it

            // Link the **new payment** via PaymentPreferenceId on the Payment side (done below)
            PaymentMethodTypeId      = originalPreference.PaymentMethodTypeId,
            PaymentMethodId          = originalPreference.PaymentMethodId,
            StatusId                 = "PMNT_NOT_PAID",                     // fresh duplicate → not yet paid
            MaxAmount                = createReq.Amount,                    // use the amount we just created with
            CreatedDate              = DateTime.UtcNow,

            // Usually null/empty in your domain, but copy if present:
            OrderItemSeqId           = originalPreference.OrderItemSeqId,
            ShipGroupSeqId           = originalPreference.ShipGroupSeqId,
            ProductPricePurposeId    = originalPreference.ProductPricePurposeId,
            // FinAccountId, SecurityCode, etc. → typically null for these cases
        };

        _context.OrderPaymentPreferences.Add(newPreference);
        await _context.SaveChangesAsync(ct);

        // ─── Critical: Update the NEW Payment to point to this new preference ───
        var newPayment = await _context.Payments
            .FirstOrDefaultAsync(p => p.PaymentId == newPaymentId, ct);

        if (newPayment != null)
        {
            newPayment.PaymentPreferenceId = newPreference.OrderPaymentPreferenceId;
            await _context.SaveChangesAsync(ct);
        }
    }
}