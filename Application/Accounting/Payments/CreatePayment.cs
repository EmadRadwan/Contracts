using Application.Accounting.Services;
using Application.Shipments.Payments;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Persistence;

namespace Application.Accounting.Payments;


public class CreatePayment
{
    public class Command : IRequest<Result<PaymentDto>>
    {
        public PaymentDto PaymentDto { get; set; }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.PaymentDto).SetValidator(new CreatePaymentValidator());
        }
    }

    public class Handler : IRequestHandler<Command, Result<PaymentDto>>
    {
        private readonly DataContext _context;
        private readonly IFinAccountService _finAccountService;
        private readonly IPaymentHelperService _paymentHelperService;


        public Handler(DataContext context, IFinAccountService finAccountService, IPaymentHelperService paymentHelperService)
        {
            _context = context;
            _finAccountService = finAccountService;
            _paymentHelperService = paymentHelperService;
        }

        public async Task<Result<PaymentDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                string finAccountTransId = null!;

                // get payment method
                var paymentMethod = await _context.PaymentMethods.SingleOrDefaultAsync(x =>
                    x.PaymentMethodId == request.PaymentDto.PaymentMethodId, cancellationToken);

                if (paymentMethod != null)
                    // check if the payment method is related to a financial account
                    if (paymentMethod.FinAccountId != null)
                    {
                        var incomingPaymentTypes = new List<string> { "CUSTOMER_DEPOSIT", "CUSTOMER_PAYMENT" };
                        var outgoingPaymentTypes = new List<string>
                        {
                            "VENDOR_PAYMENT", "CUSTOMER_REFUND", "COMMISSION_PAYMENT", "INCOME_TAX_PAYMENT",
                            "PAY_CHECK",
                            "PAYROL_PAYMENT", "PAYROLL_TAX_PAYMENT", "SALES_TAX_PAYMENT", "TAX_PAYMENT",
                            "COMMISSION_PAYMENT"
                        };
                        // based on payment type, determine if its a deposit or withdrawal using incomingPaymentTypes and outgoingPaymentTypes
                        var paymentTypeId = request.PaymentDto.PaymentTypeId;
                        var createFinAccountTransParam = new CreateFinAccountTransParam();
                        if (incomingPaymentTypes.Contains(paymentTypeId))
                            // deposit
                            // prepare data for financial account transaction creation
                            createFinAccountTransParam = new CreateFinAccountTransParam
                            {
                                FinAccountId = paymentMethod.FinAccountId,
                                FinAccountTransTypeId = "DEPOSIT",
                                StatusId = "FINACT_TRNS_CREATED",
                                PartyId = request.PaymentDto.PartyIdFrom,
                                Amount = request.PaymentDto.Amount,
                                EffectiveDate = request.PaymentDto.EffectiveDate
                            };
                        else if (outgoingPaymentTypes.Contains(paymentTypeId))
                            // withdrawal
                            // prepare data for financial account transaction creation
                            createFinAccountTransParam = new CreateFinAccountTransParam
                            {
                                FinAccountId = paymentMethod.FinAccountId,
                                FinAccountTransTypeId = "WITHDRAWAL",
                                StatusId = "FINACT_TRNS_CREATED",
                                PartyId = request.PaymentDto.PartyIdTo,
                                Amount = request.PaymentDto.Amount,
                                EffectiveDate = request.PaymentDto.EffectiveDate
                            };

                        finAccountTransId =
                            await _finAccountService.CreateFinAccountTrans(createFinAccountTransParam);
                    }

                // populate a CreatePaymentParam object
                var createPaymentParam = new CreatePaymentParam
                {
                    PaymentMethodId = request.PaymentDto.PaymentMethodId,
                    PaymentMethodTypeId = paymentMethod.PaymentMethodTypeId,
                    StatusId = "PMNT_NOT_PAID",
                    EffectiveDate = request.PaymentDto.EffectiveDate,
                    Amount = request.PaymentDto.Amount,
                    PartyIdFrom = request.PaymentDto.PartyIdFrom,
                    PartyIdTo = request.PaymentDto.PartyIdTo,
                    PaymentTypeId = request.PaymentDto.PaymentTypeId
                };
                // Create the payment itself
                var payment = await _paymentHelperService.CreatePayment(createPaymentParam);

                var addedFinAccountTran = null as EntityEntry<FinAccountTran>;

                if (paymentMethod!.FinAccountId != null)
                {
                    addedFinAccountTran = _context.ChangeTracker.Entries<FinAccountTran>()
                        .FirstOrDefault(e =>
                            e.Entity.FinAccountTransId == finAccountTransId &&
                            e.State == EntityState.Added);

                    await _context.SaveChangesAsync(cancellationToken);

                    if (addedFinAccountTran != null) addedFinAccountTran.Entity.PaymentId = payment.PaymentId;
                    payment.FinAccountTransId = finAccountTransId;
                }

                await _context.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);


                var paymentToReturn = new PaymentDto
                {
                    PaymentId = payment.PaymentId,
                    StatusId = payment.StatusId,
                    StatusDescription = "Not Paid",
                    FinAccountTransId = addedFinAccountTran != null
                        ? addedFinAccountTran.Entity.FinAccountTransId
                        : null,
                    Comments = payment.Comments,
                    Amount = payment.Amount,
                    PaymentMethodId = payment.PaymentMethodId,
                    PaymentTypeId = payment.PaymentTypeId,
                    EffectiveDate = payment.EffectiveDate
                };
                return Result<PaymentDto>.Success(paymentToReturn);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);

                return Result<PaymentDto>.Failure("Error creating Payment");
            }
        }
    }
}