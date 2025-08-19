using Application.Accounting.PaymentGroup;
using Application.Accounting.Payments;
using Application.Accounting.Services;
using Application.Accounting.Services.Models;
using Application.Shipments.Invoices;
using FluentValidation;
using MediatR;
using Persistence;

namespace Application.Accounting.PaymentGroup
{
    public class UpdatePaymentGroupMember
    {
        public class Command : IRequest<UpdatePaymentGroupMemberResult>
        {
            public UpdatePaymentGroupMemberInput Params { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Params).NotNull();
                RuleFor(x => x.Params.PaymentGroupId).NotEmpty();
                RuleFor(x => x.Params.PaymentId).NotEmpty();
                RuleFor(x => x.Params.FromDate).NotNull();
            }
        }

        public class Handler : IRequestHandler<Command, UpdatePaymentGroupMemberResult>
        {
            private readonly DataContext _context;
            private readonly IPaymentHelperService _paymentHelperService;

            public Handler(DataContext context, IPaymentHelperService paymentHelperService)
            {
                _context = context;
                _paymentHelperService = paymentHelperService;
            }

            public async Task<UpdatePaymentGroupMemberResult> Handle(Command request, CancellationToken cancellationToken)
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    var result = await _paymentHelperService.UpdatePaymentGroupMember(request.Params);
                    if (!result.IsSuccess)
                    {
                        throw new Exception(result.Message ?? "Failed to update Payment Group Member");
                    }

                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    return result;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return new UpdatePaymentGroupMemberResult
                    {
                        IsSuccess = false,
                        Message = $"Error Updating Payment Group Member: {ex.Message}"
                    };
                }
            }
        }
    }
}