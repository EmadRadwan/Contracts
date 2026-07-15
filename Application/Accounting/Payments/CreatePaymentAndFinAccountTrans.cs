 using Application.Accounting.FinAccounts;
using Application.Accounting.Services;
using Application.Core;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.Payments;

public class CreatePaymentAndFinAccountTrans
{
    public class Command : IRequest<Results<CreatePaymentAndFinAccountTransResponse>>
    {
        public CreatePaymentAndFinAccountTransRequest request { get; set; }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.request).SetValidator(new CreatePaymentAndFinAccountTransRequestValidator());
        }
    }

    public class Handler : IRequestHandler<Command, Results<CreatePaymentAndFinAccountTransResponse>>
    {
        private readonly IPaymentHelperService _paymentHelperService;
        private readonly DataContext _context;

        public Handler(DataContext context, IPaymentHelperService paymentHelperService)
        {
            _paymentHelperService = paymentHelperService;
            _context = context;
        }

        public async Task<Results<CreatePaymentAndFinAccountTransResponse>> Handle(
            Command command,
            CancellationToken cancellationToken)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var req = command.request;
                DateOnly effectiveDate = req.PaymentDate ?? DateOnly.FromDateTime(DateTime.UtcNow);


                var paymentResult = await _paymentHelperService.CreatePaymentAndFinAccountTrans(req);


                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return Results<CreatePaymentAndFinAccountTransResponse>.Success(paymentResult.Value);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);

                return Results<CreatePaymentAndFinAccountTransResponse>.Failure(
                    $"حدث خطأ أثناء إنشاء الدفعة: {ex.Message}",
                    errorCode: "UNEXPECTED_PAYMENT_CREATION_ERROR"
                );
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