using Application.Accounting.PaymentGroup;
using Application.Accounting.Services;
using Application.Accounting.Services.Models;
using Application.Shipments.Invoices;
using FluentValidation;
using MediatR;
using Persistence;

namespace Application.Accounting.PaymentGroup
{
    public class CreatePaymentGroupPayment
    {
        public class Command : IRequest<CreatePaymentGroupMemberDto>
        {
            public CreatePaymentGroupMemberDto Params { get; set; }
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

        public class Handler : IRequestHandler<Command, CreatePaymentGroupMemberDto>
        {
            private readonly DataContext _context;
            private readonly IPaymentHelperService _paymentHelperService;

            public Handler(DataContext context, IPaymentHelperService paymentHelperService)
            {
                _context = context;
                _paymentHelperService = paymentHelperService;
            }

           public async Task<CreatePaymentGroupMemberDto> Handle(Command request, CancellationToken cancellationToken)
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    var result = await _paymentHelperService.CreatePaymentGroupMember(request.Params);
                    if (!result.IsSuccess)
                    {
                        throw new Exception(result.Error);
                    }

                    var paymentGroupId = result.Value;

                    // Map paymentGroupId to CreatePaymentGroupMemberDto (assuming you need to create or retrieve the DTO)
                    var CreatePaymentGroupMemberDto = new CreatePaymentGroupMemberDto
                    {
                        PaymentGroupId = paymentGroupId,
                        // Map other properties as needed
                    };

                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    return CreatePaymentGroupMemberDto;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw new Exception("Error Creating Payment Group", ex);
                }
            }
        }
    }
}
