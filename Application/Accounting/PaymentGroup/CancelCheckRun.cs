using Application.Accounting.PaymentGroup;
using Application.Accounting.Services;
using Application.Accounting.Services.Models;
using Application.Shipments.Invoices;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.ObjectPool;
using Persistence;

namespace Application.Accounting.PaymentGroup
{
    public class CancelCheckRun
    {
        public class Command : IRequest<GeneralServiceResult<object>>
        {
            public string PaymentGroupId { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.PaymentGroupId).NotNull();
                // You can add additional validation rules here if needed,
                // e.g., RuleFor(x => x.Parameters.InvoiceId).NotEmpty();
            }
        }

        public class Handler : IRequestHandler<Command, GeneralServiceResult<object>>
        {
            private readonly DataContext _context;
            private readonly IPaymentHelperService _paymentHelperService;

            public Handler(DataContext context, IPaymentHelperService paymentHelperService)
            {
                _context = context;
                _paymentHelperService = paymentHelperService;
            }

           public async Task<GeneralServiceResult<object>> Handle(Command request, CancellationToken cancellationToken)
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    // Call the CreatePaymentGroup method and handle the Result
                    var result = await _paymentHelperService.CancelCheckRunPayments(request.PaymentGroupId);
                    if (!result.IsSuccess)
                    {
                        throw new Exception(result.ErrorMessage);
                    }

                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    return new GeneralServiceResult<object> { IsSuccess = true, ResultData = result.ResultData };
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw new Exception("Error Canceling Check Run", ex);
                }
            }
        }
    }
}
