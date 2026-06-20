using Application.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Order.SalesRequests;

public class GetApprovedSalesRequestsLov
{
    public class ApprovedSalesRequestItem
    {
        public string SalesRequestId { get; set; } = null!;
        public string? CustomerName { get; set; }
        public string ApartmentName { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public decimal? TotalPrice { get; set; }
    }

    public class ApprovedSalesRequestEnvelope
    {
        public List<ApprovedSalesRequestItem> Items { get; set; } = new();
        public int TotalCount { get; set; }
    }

    public class Query : IRequest<Result<ApprovedSalesRequestEnvelope>>
    {
        public string? SearchTerm { get; set; }
        public int Skip { get; set; } = 0;
        public int PageSize { get; set; } = 20;
    }

    public class Handler : IRequestHandler<Query, Result<ApprovedSalesRequestEnvelope>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context) => _context = context;

        public async Task<Result<ApprovedSalesRequestEnvelope>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = _context.SalesRequests
                .Where(sr => sr.StatusId == "SALES_REQUEST_APPROVED")
                .Join(_context.Products,
                    sr => sr.ProductId,
                    p => p.ProductId,
                    (sr, p) => new { sr, p })
                .Join(_context.WorkEfforts,
                    x => x.p.ProjectId,
                    we => we.WorkEffortId,
                    (x, we) => new { x.sr, x.p, we })
                .GroupJoin(_context.Parties,
                    x => x.sr.FromPartyId,
                    c => c.PartyId,
                    (x, cs) => new { x.sr, x.p, x.we, cs })
                .SelectMany(
                    x => x.cs.DefaultIfEmpty(),
                    (x, c) => new { x.sr, x.p, x.we, c })
                .Select(x => new ApprovedSalesRequestItem
                {
                    SalesRequestId = x.sr.SalesRequestId,
                    CustomerName = x.c != null ? x.c.Description : null,
                    ApartmentName = x.p.ProductName ?? x.sr.ProductId,
                    ProjectName = x.we.ProjectName ?? x.we.WorkEffortId,
                    TotalPrice = x.sr.TotalPrice
                });

            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                var lower = request.SearchTerm.Trim().ToLower();
                query = query.Where(x =>
                    x.SalesRequestId.ToLower().Contains(lower) ||
                    (x.CustomerName != null && x.CustomerName.ToLower().Contains(lower)) ||
                    x.ApartmentName.ToLower().Contains(lower) ||
                    x.ProjectName.ToLower().Contains(lower));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(x => x.SalesRequestId)
                .Skip(request.Skip)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return Result<ApprovedSalesRequestEnvelope>.Success(new ApprovedSalesRequestEnvelope
            {
                Items = items,
                TotalCount = totalCount
            });
        }
    }
}
