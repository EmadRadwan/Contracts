using Application.Accounting.Payments;
using Application.Accounting.Services;
using FluentValidation;
using MediatR;
using Persistence;

namespace Application.Accounting.PaymentGroup
{
    public class ExpirePaymentGroupMember
    {
        public class Command : IRequest<ExpirePaymentGroupMemberResult>
        {
            public ExpirePaymentGroupMemberInput Params { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Params).NotNull();
                // You can add additional validation rules here if needed,
                // e.g., RuleFor(x => x.Parameters.InvoiceId).NotEmpty();
            }
        }

        public class Handler : IRequestHandler<Command, ExpirePaymentGroupMemberResult>
        {
            private readonly DataContext _context;
            private readonly IPaymentHelperService _paymentHelperService;

            public Handler(DataContext context, IPaymentHelperService paymentHelperService)
            {
                _context = context;
                _paymentHelperService = paymentHelperService;
            }

           public async Task<ExpirePaymentGroupMemberResult> Handle(Command request, CancellationToken cancellationToken)
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    // Call the CreatePaymentGroup method and handle the Result
                    var result = await _paymentHelperService.ExpirePaymentGroupMember(request.Params);
                    if (!result.IsSuccess)
                    {
                        throw new Exception(result.Message);
                    }

                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    return new ExpirePaymentGroupMemberResult { IsSuccess = true };
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
