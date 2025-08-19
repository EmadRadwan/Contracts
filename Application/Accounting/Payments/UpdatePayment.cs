using Application.Accounting.Services;



using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Persistence;

namespace Application.Accounting.Payments;


public class UpdatePayment
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
            var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            string finAccountTransId = null!;


            try
            {
                // get payment method
                var paymentMethod = await _context.PaymentMethods.SingleOrDefaultAsync(x =>
                    x.PaymentMethodId == request.PaymentDto.PaymentMethodId, cancellationToken);

                if (paymentMethod != null)
                {
                    // check if the payment method is related to a financial account
                    if (paymentMethod.FinAccountId != null)
                    {
                        // check if a finAcctTrans exists for the payment
                        var finAccountTrans = await _context.FinAccountTrans.SingleOrDefaultAsync(x =>
                            x.PaymentId == request.PaymentDto.PaymentId, cancellationToken);
                        finAccountTransId = finAccountTrans?.FinAccountTransId;
                        if (finAccountTrans != null)
                        {
                            // populate a CreateFinAccountTransParam object
                            var createFinAccountTransParam = new CreateFinAccountTransParam
                            {
                                FinAccountId = paymentMethod.FinAccountId,
                                PartyId = finAccountTrans.FinAccountTransTypeId == "DEPOSIT"
                                    ? request.PaymentDto.PartyIdFrom
                                    : request.PaymentDto.PartyIdTo,
                                Amount = request.PaymentDto.Amount,
                                EffectiveDate = request.PaymentDto.EffectiveDate,
                                FinAccountTransTypeId = finAccountTrans.FinAccountTransTypeId,
                                FinAccountTransId = finAccountTrans.FinAccountTransId
                            };
                            await _finAccountService.UpdateFinAccountTrans(
                                createFinAccountTransParam);
                        }
                        else
                        {
                            // populate a CreateFinAccountTransParam object
                            // based on payment type, determine if its a deposit or withdrawal
                            var incomingPaymentTypes = new List<string> { "CUSTOMER_DEPOSIT", "CUSTOMER_PAYMENT" };
                            var outgoingPaymentTypes = new List<string>
                            {
                                "VENDOR_PAYMENT", "CUSTOMER_REFUND", "COMMISSION_PAYMENT", "INCOME_TAX_PAYMENT",
                                "PAY_CHECK",
                                "PAYROL_PAYMENT", "PAYROLL_TAX_PAYMENT", "SALES_TAX_PAYMENT", "TAX_PAYMENT",
                                "COMMISSION_PAYMENT"
                            };
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
                    }
                    else
                    {
                        // check if there was there a transaction from the add phase
                        // in the database and if there is then delete it
                        var finAcctTrans = _context.FinAccountTrans.SingleOrDefault(x =>
                            x.PaymentId == request.PaymentDto.PaymentId);
                        if (finAcctTrans != null) _context.FinAccountTrans.Remove(finAcctTrans);
                    }
                }
                else
                {
                    // else only update the payment, populate a CreatePaymentParam object and ignore the financial account transaction
                    var updatePaymentParam = new CreatePaymentParam
                    {
                        PaymentId = request.PaymentDto.PaymentId,
                        StatusId = request.PaymentDto.StatusId,
                        PaymentMethodId = request.PaymentDto.PaymentMethodId,
                        PaymentMethodTypeId = paymentMethod.PaymentMethodTypeId,
                        EffectiveDate = request.PaymentDto.EffectiveDate,
                        Amount = request.PaymentDto.Amount,
                        Comments = request.PaymentDto.Comments,
                        ActualCurrencyAmount = request.PaymentDto.ActualCurrencyAmount,
                        ActualCurrencyUomId = request.PaymentDto.ActualCurrencyUomId,
                        PartyIdFrom = request.PaymentDto.PartyIdFrom,
                        PartyIdTo = request.PaymentDto.PartyIdTo,
                        PaymentTypeId = request.PaymentDto.PaymentTypeId,
                        FinAccountTransId = finAccountTransId,
                    };
                    // update the payment itself
                    await _paymentHelperService.UpdatePayment(updatePaymentParam);
                }


                // populate a CreatePaymentParam object
                var updatePaymentParam2 = new CreatePaymentParam
                {
                    PaymentId = request.PaymentDto.PaymentId,
                    StatusId = request.PaymentDto.StatusId,
                    PaymentMethodId = request.PaymentDto.PaymentMethodId,
                    PaymentMethodTypeId = paymentMethod.PaymentMethodTypeId,
                    EffectiveDate = request.PaymentDto.EffectiveDate,
                    Amount = request.PaymentDto.Amount,
                    ActualCurrencyAmount = request.PaymentDto.ActualCurrencyAmount,
                    ActualCurrencyUomId = request.PaymentDto.ActualCurrencyUomId == "" ? null : request.PaymentDto.ActualCurrencyUomId,
                    Comments = request.PaymentDto.Comments,
                    PaymentRefNum = request.PaymentDto.PaymentRefNum,
                    PartyIdFrom = request.PaymentDto.PartyIdFrom,
                    PartyIdTo = request.PaymentDto.PartyIdTo,
                    PaymentTypeId = request.PaymentDto.PaymentTypeId,
                    FinAccountTransId = finAccountTransId,
                };
                // update the payment itself
                var payment = await _paymentHelperService.UpdatePayment(updatePaymentParam2);


                var addedFinAccountTran = null as EntityEntry<FinAccountTran>;


                if (paymentMethod!.FinAccountId != null && finAccountTransId != null)
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
                    FinAccountTransId = payment.FinAccountTransId,
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

                return Result<PaymentDto>.Failure("Error updating Payment");
            }
        }
    }
}