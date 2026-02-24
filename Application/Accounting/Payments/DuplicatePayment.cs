using Application.Accounting.FinAccounts;
using Application.Accounting.Services;
using Application.Core;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.Payments;

public class DuplicatePayment
{
    public class Command : IRequest<Results<CreatePaymentAndFinAccountTransResponse>>
    {
        public string OriginalPaymentId { get; set; }
    }

    public class Handler : IRequestHandler<Command, Results<CreatePaymentAndFinAccountTransResponse>>
    {
        private readonly DataContext _context;
        private readonly IPaymentHelperService _paymentHelperService;

        public Handler(DataContext context, IPaymentHelperService paymentHelperService)
        {
            _context = context;
            _paymentHelperService = paymentHelperService;
        }

        public async Task<Results<CreatePaymentAndFinAccountTransResponse>> Handle(
            Command request,
            CancellationToken ct)
        {
            var original = await _context.Payments
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PaymentId == request.OriginalPaymentId, ct);

            if (original == null)
            {
                return Results<CreatePaymentAndFinAccountTransResponse>.Failure(
                    "Original payment not found",
                    "PAYMENT_NOT_FOUND"
                );
            }

            // Prepare request object for creation (almost same fields)
            var createReq = new CreatePaymentAndFinAccountTransRequest
            {
                PaymentTypeId = original.PaymentTypeId,
                PaymentMethodId = original.PaymentMethodId,
                Amount = original.Amount,
                PaymentDate = DateTime.UtcNow, // or original.EffectiveDate ?
                Comments = original.Comments != null
                    ? $"نسخة من دفعة {original.PaymentId} – {original.Comments}"
                    : $"نسخة من دفعة {original.PaymentId}",
                PartyIdFrom = original.PartyIdFrom,
                PartyIdTo = original.PartyIdTo,
                ChequeNumber = original.ChequeNumber,
                ChequeDate = original.ChequeDate,
                OverrideGlAccountId = original.OverrideGlAccountId,
                ProjectId = original.WorkEffortId,
                CostCenterId = original.CostCenterId,
                IsBankTransfer = original.IsBankTransfer,
                StatusId = "PMNT_NOT_PAID",
                SalesRequestId = original.SalesRequestId,
            };

            var createCommand = new CreatePaymentAndFinAccountTrans.Command
            {
                request = createReq
            };

            // Reuse existing creation logic (including validations & transaction)
            var createResult = await new CreatePaymentAndFinAccountTrans.Handler(_context, _paymentHelperService)
                .Handle(createCommand, ct);

            var newPaymentId = createResult.Value.PaymentId;

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

                        OrderId = originalPreference.OrderId, // same PO / SO
                        // NO PaymentId here – that's correct, table doesn't have it

                        // Link the **new payment** via PaymentPreferenceId on the Payment side (done below)
                        PaymentMethodTypeId = originalPreference.PaymentMethodTypeId,
                        PaymentMethodId = originalPreference.PaymentMethodId,
                        StatusId = "PMNT_NOT_PAID", // fresh duplicate → not yet paid
                        MaxAmount = createReq.Amount, // use the amount we just created with
                        CreatedDate = DateTime.UtcNow,

                        // Usually null/empty in your domain, but copy if present:
                        OrderItemSeqId = originalPreference.OrderItemSeqId,
                        ShipGroupSeqId = originalPreference.ShipGroupSeqId,
                        ProductPricePurposeId = originalPreference.ProductPricePurposeId,
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

            return createResult;
        }
    }
}