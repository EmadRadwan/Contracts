using Application.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Projects;

public class GetSalesCommissionDefaults
{
    public class CommissionDefaults
    {
        public string? ProjectId { get; set; }
        public string? SaleTypeId { get; set; }
        public decimal SalePrice { get; set; }
        public decimal SalesRepPercent { get; set; }
        public decimal ManagerPercent { get; set; }
        public decimal? ExternalCompanyPercent { get; set; }
        public decimal? ExternalSalesRepPercent { get; set; }
        public decimal? ExternalManagerPercent { get; set; }
    }

    public class Query : IRequest<Result<CommissionDefaults>>
    {
        public string SalesRequestId { get; set; } = null!;
    }

    public class Handler : IRequestHandler<Query, Result<CommissionDefaults>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context) => _context = context;

        public async Task<Result<CommissionDefaults>> Handle(Query request, CancellationToken cancellationToken)
        {
            var sr = await _context.SalesRequests
                .Where(x => x.SalesRequestId == request.SalesRequestId)
                .Join(_context.Products,
                    s => s.ProductId,
                    p => p.ProductId,
                    (s, p) => new { s, p })
                .FirstOrDefaultAsync(cancellationToken);

            if (sr == null)
                return Result<CommissionDefaults>.Failure("Sales request not found");

            var projectId = sr.p.ProjectId;
            var salePrice = sr.s.TotalPrice ?? 0;

            var rate = await _context.ProjectCommissionRates
                .Where(r => r.ProjectId == projectId)
                .FirstOrDefaultAsync(cancellationToken);

            var defaults = new CommissionDefaults
            {
                ProjectId = projectId,
                SalePrice = salePrice,
                SaleTypeId = rate?.SaleTypeId,
                SalesRepPercent = rate?.SalesRepPercent ?? 0,
                ManagerPercent = rate?.ManagerPercent ?? 0,
                ExternalCompanyPercent = rate?.ExternalCompanyPercent,
                ExternalSalesRepPercent = rate?.ExternalSalesRepPercent,
                ExternalManagerPercent = rate?.ExternalManagerPercent
            };

            return Result<CommissionDefaults>.Success(defaults);
        }
    }
}
