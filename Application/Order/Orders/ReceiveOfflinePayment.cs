


using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Order.Orders
{
    public class ReceiveOfflinePayment
    {
        public class Command : IRequest<Result<string>>
        {
            public ReceiveOfflinePaymentInput ReceiveOfflinePaymentInput { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.ReceiveOfflinePaymentInput).NotNull();
                RuleFor(x => x.ReceiveOfflinePaymentInput.OrderId).NotEmpty();
                RuleFor(x => x.ReceiveOfflinePaymentInput.PartyId).NotEmpty();
                RuleForEach(x => x.ReceiveOfflinePaymentInput.PaymentDetails).SetValidator(new PaymentDetailValidator());
            }
        }

        public class PaymentDetailValidator : AbstractValidator<PaymentDetail>
        {
            public PaymentDetailValidator()
            {
                RuleFor(x => x.Amount).GreaterThan(0);
                RuleFor(x => x.PaymentMethodId).NotEmpty().When(x => string.IsNullOrEmpty(x.PaymentMethodTypeId));
                RuleFor(x => x.PaymentMethodTypeId).NotEmpty().When(x => string.IsNullOrEmpty(x.PaymentMethodId));
            }
        }

        public class Handler : IRequestHandler<Command, Result<string>>
        {
            private readonly DataContext _context;
            private readonly IOrderHelperService _orderHelperService;
            private readonly ILogger<Handler> _logger;

            public Handler(DataContext context, IOrderHelperService orderHelperService, ILogger<Handler> logger)
            {
                _context = context;
                _orderHelperService = orderHelperService;
                _logger = logger;
            }

            public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
            {
                var input = request.ReceiveOfflinePaymentInput;
                try
                {
                    // Call the ReceiveOfflinePayment function from the OrderHelperService
                    var result = await _orderHelperService.ReceiveOfflinePayment(input);

                    if (result == "success")
                    {
                        return Result<string>.Success(result);
                    }

                    _logger.LogError("Failed to process offline payment: {0}", result);
                    return Result<string>.Failure(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error in ReceiveOfflinePayment handler: {0}", ex.Message);
                    return Result<string>.Failure("OrderErrorProcessingOfflinePayments");
                }
            }
        }
    }
}
