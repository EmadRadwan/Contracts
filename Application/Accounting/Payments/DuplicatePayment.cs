using Application.Accounting.FinAccounts;
using Application.Accounting.Services;
using Application.Core;
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
                PaymentTypeId      = original.PaymentTypeId,
                PaymentMethodId    = original.PaymentMethodId,
                Amount             = original.Amount,
                PaymentDate        = DateTime.UtcNow,           // or original.EffectiveDate ?
                Comments           = original.Comments != null 
                                     ? $"نسخة من دفعة {original.PaymentId} – {original.Comments}"
                                     : $"نسخة من دفعة {original.PaymentId}",
                PartyIdFrom        = original.PartyIdFrom,
                PartyIdTo          = original.PartyIdTo,
                ChequeNumber       = original.ChequeNumber,
                ChequeDate         = original.ChequeDate,
                OverrideGlAccountId= original.OverrideGlAccountId,
                ProjectId          = original.WorkEffortId,
                CostCenterId       = original.CostCenterId,
                IsBankTransfer     = original.IsBankTransfer,
                StatusId          = "PMNT_NOT_PAID", 
                SalesRequestId       = original.SalesRequestId,
            };

            var createCommand = new CreatePaymentAndFinAccountTrans.Command 
            { 
                request = createReq 
            };

            // Reuse existing creation logic (including validations & transaction)
            var result = await new CreatePaymentAndFinAccountTrans.Handler(_context, _paymentHelperService)
                .Handle(createCommand, ct);

            return result;
        }
    }
}