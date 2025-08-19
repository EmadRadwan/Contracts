using MediatR;
using Microsoft.Extensions.Logging;
using Application.Accounting.Services;
using Application.Accounting.Services.Models; // REFACTOR: Added for RemovePaymentApplicationResult
using Application.Core;

namespace Application.Accounting.Payments
{
    public class RemovePaymentApplication
    {
        public class Command : IRequest<Results<RemovePaymentApplicationResult>>
        {
            public string PaymentApplicationId { get; set; }
        }

        public class Handler : IRequestHandler<Command, Results<RemovePaymentApplicationResult>>
        {
            private readonly IPaymentApplicationService _paymentApplicationService;
            private readonly ILogger<Handler> _logger;

            public Handler(IPaymentApplicationService paymentApplicationService, ILogger<Handler> logger)
            {
                _paymentApplicationService = paymentApplicationService;
                _logger = logger;
            }

            public async Task<Results<RemovePaymentApplicationResult>> Handle(Command request,
                CancellationToken cancellationToken)
            {
                try
                {
                    // REFACTOR: Delegate deletion to IPaymentApplicationService
                    // Technical: Calls RemovePaymentApplication, expecting GeneralServiceResult<Models.RemovePaymentApplicationResult>
                    // Business Purpose: Encapsulates business logic in the service
                    var serviceResult =
                        await _paymentApplicationService.RemovePaymentApplication(request.PaymentApplicationId);

                    // REFACTOR: Map GeneralServiceResult to Results
                    // Technical: Converts GeneralServiceResult to Results, using Models.RemovePaymentApplicationResult
                    // Business Purpose: Ensures consistent return type for frontend
                    if (serviceResult.IsSuccess)
                    {
                        return Results<RemovePaymentApplicationResult>.Success(serviceResult.ResultData);
                    }

                    return Results<RemovePaymentApplicationResult>.Failure(
                        serviceResult.ErrorMessage,
                        "SERVICE_ERROR" // REFACTOR: Default ErrorCode for service errors
                    );
                }
                catch (Exception ex)
                {
                    // REFACTOR: Use Results.Failure for error handling
                    // Technical: Logs exception and returns Results.Failure with ErrorCode
                    // Business Purpose: Tracks deletion failures for auditing
                    _logger.LogError(ex,
                        "Error in RemovePaymentApplication for paymentApplicationId: {PaymentApplicationId}",
                        request.PaymentApplicationId);
                    return Results<RemovePaymentApplicationResult>.Failure(
                        "Failed to remove payment application.",
                        "HANDLER_ERROR" // REFACTOR: Specific ErrorCode for handler exceptions
                    );
                }
            }
        }
    }
}