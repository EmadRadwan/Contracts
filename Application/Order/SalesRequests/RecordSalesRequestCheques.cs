using Application.Accounting.Payments;
using Application.Accounting.Services;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Order.SalesRequests;

// Attaches physically-received cheque details (bank, cheque number, cheque date, amount)
// onto existing not-yet-collected installment/maintenance Payments for a Sales Request.
//
// Deliberately does NOT post any GL entries - that refactor (replacing the lump-sum
// "IsChequesDelivered" posting in ApproveSalesRequest.cs with one 124410 debit per cheque)
// is a separate follow-up task. This command only records which cheques were received.
public class RecordSalesRequestCheques
{
    public class ChequeEntry
    {
        public string PaymentId { get; set; } = null!;
        public string PaymentMethodId { get; set; } = null!;
        public string ChequeNumber { get; set; } = null!;
        public DateOnly ChequeDate { get; set; }
        public decimal Amount { get; set; }
    }

    public class Command : IRequest<Result<List<ListChequeableSalesRequestPayments.ChequeablePaymentDto>>>
    {
        public string SalesRequestId { get; set; } = null!;
        public List<ChequeEntry> Cheques { get; set; } = new();
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.SalesRequestId).NotEmpty();
            RuleFor(x => x.Cheques).NotEmpty();
            RuleForEach(x => x.Cheques).ChildRules(cheque =>
            {
                cheque.RuleFor(c => c.PaymentId).NotEmpty();
                cheque.RuleFor(c => c.PaymentMethodId).NotEmpty();
                cheque.RuleFor(c => c.ChequeNumber).NotEmpty();
                cheque.RuleFor(c => c.ChequeDate).NotEmpty();
                cheque.RuleFor(c => c.Amount).GreaterThan(0);
            });
        }
    }

    public class Handler : IRequestHandler<Command, Result<List<ListChequeableSalesRequestPayments.ChequeablePaymentDto>>>
    {
        private readonly DataContext _context;
        private readonly IPaymentHelperService _paymentHelperService;

        public Handler(DataContext context, IPaymentHelperService paymentHelperService)
        {
            _context = context;
            _paymentHelperService = paymentHelperService;
        }

        public async Task<Result<List<ListChequeableSalesRequestPayments.ChequeablePaymentDto>>> Handle(
            Command request, CancellationToken ct)
        {
            var sr = await _context.SalesRequests
                .FirstOrDefaultAsync(x => x.SalesRequestId == request.SalesRequestId, ct);

            if (sr == null)
                return Result<List<ListChequeableSalesRequestPayments.ChequeablePaymentDto>>.Failure(
                    "Sales request not found");

            var paymentIds = request.Cheques.Select(c => c.PaymentId).ToList();

            var payments = await _context.Payments
                .Where(p => paymentIds.Contains(p.PaymentId))
                .ToListAsync(ct);

            foreach (var cheque in request.Cheques)
            {
                var payment = payments.FirstOrDefault(p => p.PaymentId == cheque.PaymentId);

                if (payment == null)
                    return Result<List<ListChequeableSalesRequestPayments.ChequeablePaymentDto>>.Failure(
                        $"Payment {cheque.PaymentId} not found");

                if (payment.SalesRequestId != request.SalesRequestId)
                    return Result<List<ListChequeableSalesRequestPayments.ChequeablePaymentDto>>.Failure(
                        $"Payment {cheque.PaymentId} does not belong to sales request {request.SalesRequestId}");

                if (payment.StatusId != "PMNT_NOT_PAID")
                    return Result<List<ListChequeableSalesRequestPayments.ChequeablePaymentDto>>.Failure(
                        $"Payment {cheque.PaymentId} is not in a state that can receive a cheque");

                if (!string.IsNullOrEmpty(payment.ChequeNumber))
                    return Result<List<ListChequeableSalesRequestPayments.ChequeablePaymentDto>>.Failure(
                        $"Payment {cheque.PaymentId} already has a cheque attached");
            }

            var paymentMethodIds = request.Cheques.Select(c => c.PaymentMethodId).Distinct().ToList();

            var existingPaymentMethodIds = await _context.PaymentMethods
                .Where(pm => paymentMethodIds.Contains(pm.PaymentMethodId))
                .Select(pm => pm.PaymentMethodId)
                .ToListAsync(ct);

            var missingPaymentMethodId = paymentMethodIds.FirstOrDefault(id => !existingPaymentMethodIds.Contains(id));
            if (missingPaymentMethodId != null)
                return Result<List<ListChequeableSalesRequestPayments.ChequeablePaymentDto>>.Failure(
                    $"Bank/payment method {missingPaymentMethodId} not found");

            // Prevent the same cheque (bank + number) from being recorded twice, either within
            // this request or against an already-recorded cheque elsewhere in the system.
            var duplicateInRequest = request.Cheques
                .GroupBy(c => (c.PaymentMethodId, c.ChequeNumber))
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicateInRequest != null)
                return Result<List<ListChequeableSalesRequestPayments.ChequeablePaymentDto>>.Failure(
                    $"Cheque {duplicateInRequest.Key.ChequeNumber} is listed more than once in this request");

            var chequeNumbers = request.Cheques.Select(c => c.ChequeNumber).ToList();

            var existingChequeNumbers = await _context.Payments
                .Where(p => chequeNumbers.Contains(p.ChequeNumber))
                .Select(p => new { p.ChequeNumber, p.PaymentMethodId })
                .ToListAsync(ct);

            foreach (var cheque in request.Cheques)
            {
                if (existingChequeNumbers.Any(e =>
                        e.ChequeNumber == cheque.ChequeNumber && e.PaymentMethodId == cheque.PaymentMethodId))
                {
                    return Result<List<ListChequeableSalesRequestPayments.ChequeablePaymentDto>>.Failure(
                        $"Cheque {cheque.ChequeNumber} is already recorded against that bank");
                }
            }

            await using var transaction = await _context.Database.BeginTransactionAsync(ct);

            try
            {
                foreach (var cheque in request.Cheques)
                {
                    await _paymentHelperService.UpdatePayment(new CreatePaymentParam
                    {
                        PaymentId = cheque.PaymentId,
                        PaymentMethodId = cheque.PaymentMethodId,
                        ChequeNumber = cheque.ChequeNumber,
                        ChequeDate = cheque.ChequeDate,
                        Amount = cheque.Amount
                    });
                }

                await _context.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                return Result<List<ListChequeableSalesRequestPayments.ChequeablePaymentDto>>.Failure(
                    $"Failed to record cheques: {ex.Message}");
            }

            var updated = payments
                .Where(p => paymentIds.Contains(p.PaymentId))
                .Select(p => new ListChequeableSalesRequestPayments.ChequeablePaymentDto
                {
                    PaymentId = p.PaymentId,
                    PaymentTypeId = p.PaymentTypeId,
                    DueDate = p.EffectiveDate,
                    Amount = p.Amount,
                    Comments = p.Comments
                })
                .ToList();

            return Result<List<ListChequeableSalesRequestPayments.ChequeablePaymentDto>>.Success(updated);
        }
    }
}
