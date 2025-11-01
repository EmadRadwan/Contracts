using Application.Accounting.Services;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Accounting.Payments
{
    public class CreatePaymentApplication
    {
        // --------------------------------------------------------------------
        // 1. COMMAND
        // --------------------------------------------------------------------
        public class Command : IRequest<Result<PaymentApplicationParam>>
        {
            public PaymentApplicationParam Param { get; set; } = null!;
        }

        // --------------------------------------------------------------------
        // 2. VALIDATOR
        // --------------------------------------------------------------------
        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Param.AmountApplied).GreaterThan(0)
                    .When(x => x.Param.AmountApplied.HasValue);
            }
        }

        // --------------------------------------------------------------------
        // 3. HANDLER – with transaction + logging + persistence
        // --------------------------------------------------------------------
        public class Handler : IRequestHandler<Command, Result<PaymentApplicationParam>>
        {
            private readonly DataContext _context;
            private readonly IPaymentApplicationService _paymentApplicationService;
            private readonly ILogger<Handler> _logger;

            public Handler(
                DataContext context,
                IPaymentApplicationService paymentApplicationService,
                ILogger<Handler> logger)
            {
                _context = context;
                _paymentApplicationService = paymentApplicationService;
                _logger = logger;
            }

            public async Task<Result<PaymentApplicationParam>> Handle(Command request, CancellationToken cancellationToken)
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    // ----------------------------------------------------------------
                    // Delegate logic to the PaymentApplicationService
                    // ----------------------------------------------------------------
                    var result = await _paymentApplicationService.CreatePaymentApplication(request.Param);

                    // Save any tracked changes (if the service created entities)
                    await _context.SaveChangesAsync(cancellationToken);

                    await transaction.CommitAsync(cancellationToken);

                    // ----------------------------------------------------------------
                    // Return success result with the created PaymentApplicationParam
                    // ----------------------------------------------------------------
                    return Result<PaymentApplicationParam>.Success(result);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);

                    _logger.LogError(ex,
                        "Error creating payment application for PaymentId: {PaymentId}",
                        request.Param.PaymentId);

                    // Optional: map known exception messages to codes
                    var errorCode = ex.Message.Contains("ParameterMissing")
                        ? "AccountingPaymentApplicationParameterMissing"
                        : ex.Message.Contains("NotFound")
                            ? "PaymentNotFound"
                            : "UNEXPECTED_ERROR";

                    return Result<PaymentApplicationParam>.Failure(errorCode);
                }
            }
        }
    }
}
