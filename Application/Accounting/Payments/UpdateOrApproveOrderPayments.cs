using Application.Interfaces;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.Payments;


public class UpdateOrApproveSalesOrderPayments
{
    public class Command : IRequest<Result<PaymentsDto>>
    {
        public PaymentsDto PaymentsDto { get; set; }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.PaymentsDto).SetValidator(new PaymentValidator());
        }
    }

    public class Handler : IRequestHandler<Command, Result<PaymentsDto>>
    {
        private readonly DataContext _context;
        private readonly IUserAccessor _userAccessor;

        public Handler(DataContext context, IUserAccessor userAccessor)
        {
            _userAccessor = userAccessor;
            _context = context;
        }

        public async Task<Result<PaymentsDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var transaction = _context.Database.BeginTransaction();

            var user = await _context.Users.FirstOrDefaultAsync(x =>
                x.UserName == _userAccessor.GetUsername());

            var stamp = DateTime.UtcNow;

            // select order
            var order = await _context.OrderHeaders.SingleOrDefaultAsync(x => x.OrderId == request.PaymentsDto.OrderId);


            if (request.PaymentsDto.Payments is { Count: > 0 })
            {
                // get bill_to_customer for this order
                var orderRoleBillToCustomer = _context.OrderRoles.SingleOrDefault(x =>
                    x.OrderId == request.PaymentsDto.OrderId
                    && x.RoleTypeId == "BILL_TO_CUSTOMER");

                // get bill from vendor for this order
                var orderRoleBillFromVendor = _context.OrderRoles.SingleOrDefault(x =>
                    x.OrderId == request.PaymentsDto.OrderId
                    && x.RoleTypeId == "BILL_FROM_VENDOR");

                foreach (var updatedPayment in request.PaymentsDto.Payments)
                {
                    // get saved payments for the order from Payments table
                    var savedPayment = await _context.Payments
                        .Where(x => x.PaymentId == updatedPayment.PaymentId).FirstOrDefaultAsync();


                    if (savedPayment != null)
                    {
                        var orderStatuses = _context.OrderStatuses.Where(x => x.OrderId
                            == request.PaymentsDto.OrderId && x.OrderPaymentPreferenceId != null).ToList();

                        var orderPaymentPreferences = _context.OrderPaymentPreferences.Where(x => x.OrderId
                            == request.PaymentsDto.OrderId).ToList();

                        if (updatedPayment.IsPaymentDeleted)
                        {
                            _context.OrderStatuses.RemoveRange(orderStatuses);
                            _context.OrderPaymentPreferences.RemoveRange(orderPaymentPreferences);

                            _context.Payments.Remove(savedPayment);
                        }
                        else
                        {
                            // Since many parameters may have been changes, so undo the previous changes and then apply as a new record
                            _context.OrderStatuses.RemoveRange(orderStatuses);
                            _context.OrderPaymentPreferences.RemoveRange(orderPaymentPreferences);

                            // get order payment preference sequence from sequence value item
                            var orderPaymentPreferenceSequenceRecord = await _context.SequenceValueItems
                                .Where(x => x.SeqName == "OrderPaymentPreference").SingleOrDefaultAsync();
                            // increment order payment preference sequence
                            var newOrderPaymentPreferenceSequence = orderPaymentPreferenceSequenceRecord.SeqId + 1;
                            // update order payment preference sequence
                            orderPaymentPreferenceSequenceRecord.SeqId = newOrderPaymentPreferenceSequence;

                            // create order payment preference
                            var orderPaymentPreference = new OrderPaymentPreference
                            {
                                OrderPaymentPreferenceId = newOrderPaymentPreferenceSequence.ToString(),
                                OrderId = request.PaymentsDto.OrderId,
                                PaymentMethodTypeId = updatedPayment.PaymentMethodTypeId,
                                StatusId = "PMNT_NOT_PAID",
                                MaxAmount = request.PaymentsDto.GrandTotal,
                                CreatedStamp = stamp,
                                LastUpdatedStamp = stamp
                            };
                            _context.OrderPaymentPreferences.Add(orderPaymentPreference);

                            var orderPaymentNotReceivedStatus = new OrderStatus
                            {
                                OrderStatusId = Guid.NewGuid().ToString(),
                                StatusId = "PMNT_NOT_PAID",
                                OrderPaymentPreferenceId = newOrderPaymentPreferenceSequence.ToString(),
                                OrderId = request.PaymentsDto.OrderId,
                                StatusDatetime = stamp,
                                LastUpdatedStamp = stamp,
                                CreatedStamp = stamp
                            };
                            _context.OrderStatuses.Add(orderPaymentNotReceivedStatus);


                            savedPayment.PaymentMethodTypeId = updatedPayment.PaymentMethodTypeId;
                            savedPayment.PaymentPreferenceId = newOrderPaymentPreferenceSequence.ToString();
                            savedPayment.PartyIdFrom = orderRoleBillToCustomer.PartyId;
                            savedPayment.PartyIdTo = orderRoleBillFromVendor.PartyId;
                            savedPayment.RoleTypeIdTo = orderRoleBillToCustomer.RoleTypeId;
                            savedPayment.LastUpdatedStamp = stamp;
                            savedPayment.EffectiveDate = stamp;
                            savedPayment.Amount = updatedPayment.Amount;
                            savedPayment.StatusId = "PMNT_NOT_PAID";
                        }
                    }
                    // new payment
                    else
                    {
                        // get payment record sequence from sequence value item 
                        var paymentSequenceRecord = await _context.SequenceValueItems
                            .Where(x => x.SeqName == "Payment").SingleOrDefaultAsync();
                        // increment payment sequence
                        var newPaymentSequence = paymentSequenceRecord.SeqId + 1;
                        // update payment sequence
                        paymentSequenceRecord.SeqId = newPaymentSequence;

                        // get order payment preference sequence from sequence value item
                        var orderPaymentPreferenceSequenceRecord = await _context.SequenceValueItems
                            .Where(x => x.SeqName == "OrderPaymentPreference").SingleOrDefaultAsync();
                        // increment order payment preference sequence
                        var newOrderPaymentPreferenceSequence = orderPaymentPreferenceSequenceRecord.SeqId + 1;
                        // update order payment preference sequence
                        orderPaymentPreferenceSequenceRecord.SeqId = newOrderPaymentPreferenceSequence;

                        // create order payment preference
                        var orderPaymentPreference = new OrderPaymentPreference
                        {
                            OrderPaymentPreferenceId = newOrderPaymentPreferenceSequence.ToString(),
                            OrderId = request.PaymentsDto.OrderId,
                            PaymentMethodTypeId = updatedPayment.PaymentMethodTypeId,
                            StatusId = "PMNT_NOT_PAID",
                            MaxAmount = request.PaymentsDto.GrandTotal,
                            CreatedStamp = stamp,
                            LastUpdatedStamp = stamp
                        };
                        _context.OrderPaymentPreferences.Add(orderPaymentPreference);

                        var orderPaymentNotReceivedStatus = new OrderStatus
                        {
                            OrderStatusId = Guid.NewGuid().ToString(),
                            StatusId = "PMNT_NOT_PAID",
                            OrderPaymentPreferenceId = newOrderPaymentPreferenceSequence.ToString(),
                            OrderId = request.PaymentsDto.OrderId,
                            StatusDatetime = stamp,
                            LastUpdatedStamp = stamp,
                            CreatedStamp = stamp
                        };
                        _context.OrderStatuses.Add(orderPaymentNotReceivedStatus);

                        var newPayment = new Payment
                        {
                            PaymentId = newPaymentSequence.ToString(),
                            PaymentTypeId = "CUSTOMER_PAYMENT",
                            PaymentMethodTypeId = updatedPayment.PaymentMethodTypeId,
                            PaymentPreferenceId = newOrderPaymentPreferenceSequence.ToString(),
                            PaymentMethodId = null,
                            PaymentGatewayResponseId = null,
                            StatusId = "PMNT_NOT_PAID",
                            PartyIdFrom = orderRoleBillToCustomer.PartyId,
                            PartyIdTo = orderRoleBillFromVendor.PartyId,
                            RoleTypeIdTo = orderRoleBillToCustomer.RoleTypeId,
                            PaymentRefNum = null,
                            EffectiveDate = stamp,
                            Amount = updatedPayment.Amount,
                            CurrencyUomId = order.CurrencyUom,
                            Comments = null,
                            LastUpdatedStamp = stamp,
                            CreatedStamp = stamp
                        };
                        _context.Payments.Add(newPayment);
                    }
                }
            }
            else
            {
                // get order payment preference that was created when order was created
                var orderPaymentPreference = await _context.OrderPaymentPreferences
                    .Where(x => x.OrderId == request.PaymentsDto.OrderId).FirstOrDefaultAsync();
                // update order payment preference
                orderPaymentPreference.MaxAmount = request.PaymentsDto.GrandTotal;
                orderPaymentPreference.LastUpdatedStamp = stamp;
            }


            var result = await _context.SaveChangesAsync() > 0;

            if (!result)
            {
                transaction.Rollback();
                return Result<PaymentsDto>.Failure("Failed to update payments");
            }

            transaction.Commit();

            var paymentResult = new PaymentsDto
            {
                OrderId = request.PaymentsDto.OrderId,
                StatusDescription = "Update"
            };


            return Result<PaymentsDto>.Success(paymentResult);
        }
    }
}