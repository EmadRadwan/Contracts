
using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Accounting.FinAccounts;
using Application.Accounting.Services;
using Application.Shipments.Payments;
using FluentValidation;
using MediatR;
using Persistence;

namespace Application.Accounting.Payments;

public class CreatePaymentAndFinAccountTrans
{
    public class Command : IRequest<Result<CreatePaymentAndFinAccountTransResponse>>
    {
        public CreatePaymentAndFinAccountTransRequest Request { get; set; }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.Request).SetValidator(new CreatePaymentAndFinAccountTransRequestValidator());
        }
    }

    public class Handler : IRequestHandler<Command, Result<CreatePaymentAndFinAccountTransResponse>>
    {
        private readonly IPaymentHelperService _paymentHelperService;
        private readonly DataContext _context;


        public Handler(DataContext context, IPaymentHelperService paymentHelperService)
        {
            _paymentHelperService = paymentHelperService;
            _context = context;
        }

        public async Task<Result<CreatePaymentAndFinAccountTransResponse>> Handle(Command command, CancellationToken cancellationToken)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var result = await _paymentHelperService.CreatePaymentAndFinAccountTrans(command.Request);
                await _context.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                return result;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);

                return Result<CreatePaymentAndFinAccountTransResponse>.Failure($"Error creating Payment and FinAccountTrans: {ex.Message}");
            }
        }
    }
}

public class CreatePaymentAndFinAccountTransRequestValidator : AbstractValidator<CreatePaymentAndFinAccountTransRequest>
{
    public CreatePaymentAndFinAccountTransRequestValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.StatusId).NotEmpty();
    }
}