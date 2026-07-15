using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Order.SalesRequests;

// Lists the SalesRequest's not-yet-collected Payments (installments / maintenance deposit)
// that don't have cheque details attached yet - i.e. the rows the "Record Received Cheques"
// grid lets the user attach a physical cheque to.
public class ListChequeableSalesRequestPayments
{
    public class Query : IRequest<List<ChequeablePaymentDto>>
    {
        public string SalesRequestId { get; set; } = null!;
    }

    public class ChequeablePaymentDto
    {
        public string PaymentId { get; set; } = null!;
        public string? PaymentTypeId { get; set; }
        public DateOnly? DueDate { get; set; }
        public decimal Amount { get; set; }
        public string? Comments { get; set; }
    }

    public class Handler : IRequestHandler<Query, List<ChequeablePaymentDto>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context) => _context = context;

        public async Task<List<ChequeablePaymentDto>> Handle(Query request, CancellationToken ct)
        {
            return await _context.Payments
                .Where(p => p.SalesRequestId == request.SalesRequestId
                            && p.StatusId == "PMNT_NOT_PAID"
                            && p.ChequeNumber == null)
                .OrderBy(p => p.EffectiveDate)
                .Select(p => new ChequeablePaymentDto
                {
                    PaymentId = p.PaymentId,
                    PaymentTypeId = p.PaymentTypeId,
                    DueDate = p.EffectiveDate,
                    Amount = p.Amount,
                    Comments = p.Comments
                })
                .ToListAsync(ct);
        }
    }
}
