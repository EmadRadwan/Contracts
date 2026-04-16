using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Order.SalesRequests;

// REFACTOR: New query to fetch saved custom installments for a specific sales request
// Purpose: Dedicated endpoint for loading installments when editing/viewing a request.
// Improves separation of concerns – list query stays lightweight, installments loaded on-demand.
// Enables efficient caching in frontend RTK Query hook.
public class GetInstallmentsQuery
{
    public class Query : IRequest<List<InstallmentDto>>
    {
        public string SalesRequestId { get; set; } = null!;
    }

    // REFACTOR: Simple DTO matching frontend expectations
    // Purpose: Avoids exposing domain entity directly and keeps response focused.
    // Improves compatibility with PaymentPlanModal (dueDate as string, isAdvance flag).
    public class InstallmentDto
    {
        public int InstallmentNumber { get; set; }
        public string DueDate { get; set; } = null!; // YYYY-MM-DD format
        public decimal Amount { get; set; }
        public bool IsAdvance { get; set; }
    }

    public class Handler : IRequestHandler<Query, List<InstallmentDto>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context) => _context = context;

        public async Task<List<InstallmentDto>> Handle(Query request, CancellationToken ct)
        {
            // REFACTOR: Direct, efficient query on SalesRequestInstallments table
            // Purpose: Fast retrieval of only needed fields, ordered correctly.
            // Improves performance – no unnecessary joins or heavy projections.
            var installments = await _context.SalesRequestInstallments
                .Where(i => i.SalesRequestId == request.SalesRequestId)
                .OrderBy(i => i.InstallmentNumber)
                .Select(i => new InstallmentDto
                {
                    InstallmentNumber = i.InstallmentNumber,
                    DueDate = i.DueDate.ToString(), // Consistent format for frontend date input
                    Amount = i.Amount,
                    IsAdvance = i.IsAdvance
                })
                .ToListAsync(ct);

            return installments;
        }
    }
}