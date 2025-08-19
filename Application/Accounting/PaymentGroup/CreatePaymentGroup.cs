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
    public class CreatePaymentGroup
    {
        public class Command : IRequest<PaymentGroupDto>
        {
            public CreatePaymentGroupDto Params { get; set; }
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

        public class Handler : IRequestHandler<Command, PaymentGroupDto>
        {
            private readonly DataContext _context;
            private readonly IPaymentHelperService _paymentHelperService;

            public Handler(DataContext context, IPaymentHelperService paymentHelperService)
            {
                _context = context;
                _paymentHelperService = paymentHelperService;
            }

           public async Task<PaymentGroupDto> Handle(Command request, CancellationToken cancellationToken)
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    // Call the CreatePaymentGroup method and handle the Result
                    var result = await _paymentHelperService.CreatePaymentGroup(request.Params);
                    if (!result.IsSuccess)
                    {
                        throw new Exception(result.Error);
                    }

                    var paymentGroupId = result.Value;

                    // Map paymentGroupId to PaymentGroupDto (assuming you need to create or retrieve the DTO)
                    var paymentGroupDto = new PaymentGroupDto
                    {
                        PaymentGroupId = paymentGroupId,
                        // Map other properties as needed
                    };

                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    return paymentGroupDto;
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
