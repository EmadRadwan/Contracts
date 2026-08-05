using Application.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Projects;

public class GetSalesCommissionDefaults
{
    public class CommissionRateDefaults
    {
        public decimal SalesRepPercent { get; set; }
        public decimal ManagerPercent { get; set; }
        public decimal? ExternalCompanyPercent { get; set; }
        public decimal? ExternalSalesRepPercent { get; set; }
        public decimal? ExternalManagerPercent { get; set; }
    }

    public class CommissionDefaults
    {
        public string? ProjectId { get; set; }
        public string? ProjectName { get; set; }
        public string? ApartmentName { get; set; }
        public decimal SalePrice { get; set; }
        public decimal CollectedAmount { get; set; }
        public decimal CollectedRatio { get; set; }
        public string? ExistingCommissionId { get; set; }

        // Key = SaleTypeId (COMM_SALE_DIRECT | COMM_SALE_PERSONAL | COMM_SALE_INDIRECT)
        public Dictionary<string, CommissionRateDefaults> Rates { get; set; } = new();
    }

    public class Query : IRequest<Result<CommissionDefaults>>
    {
        public string SalesRequestId { get; set; } = null!;

        // When provided, only the rate for this sale type is returned; otherwise all rates are returned
        public string? SaleTypeId { get; set; }
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
            var apartmentName = sr.p.ProductName;
            var salePrice = sr.s.TotalPrice ?? 0;

            var projectName = projectId != null
                ? await _context.WorkEfforts
                    .Where(w => w.WorkEffortId == projectId)
                    .Select(w => w.ProjectName)
                    .FirstOrDefaultAsync(cancellationToken)
                : null;

            var collectedAmount = await _context.Payments
                .Where(p => p.SalesRequestId == request.SalesRequestId
                            && p.Amount > 0
                            && (p.PaymentTypeId == "RECEIPT_ADVANCE_PAYMENT"
                                || p.PaymentTypeId == "RECEIPT_DUE_INSTALLMENT")
                            && p.StatusId == "PMNT_RECEIVED")
                .SumAsync(p => (decimal?)p.Amount, cancellationToken) ?? 0m;

            var collectedRatio = salePrice > 0 ? collectedAmount / salePrice : 0m;

            var existingCommissionId = await _context.SalesCommissions
                .Where(c => c.SalesRequestId == request.SalesRequestId)
                .Select(c => c.SalesCommissionId)
                .FirstOrDefaultAsync(cancellationToken);

            var ratesQuery = _context.ProjectCommissionRates
                .Where(r => r.ProjectId == projectId);

            if (!string.IsNullOrEmpty(request.SaleTypeId))
                ratesQuery = ratesQuery.Where(r => r.SaleTypeId == request.SaleTypeId);

            var ratesList = await ratesQuery.ToListAsync(cancellationToken);

            var rates = ratesList.ToDictionary(
                r => r.SaleTypeId,
                r => new CommissionRateDefaults
                {
                    SalesRepPercent = r.SalesRepPercent,
                    ManagerPercent = r.ManagerPercent,
                    ExternalCompanyPercent = r.ExternalCompanyPercent,
                    ExternalSalesRepPercent = r.ExternalSalesRepPercent,
                    ExternalManagerPercent = r.ExternalManagerPercent,
                });

            return Result<CommissionDefaults>.Success(new CommissionDefaults
            {
                ProjectId = projectId,
                ProjectName = projectName,
                ApartmentName = apartmentName,
                SalePrice = salePrice,
                CollectedAmount = collectedAmount,
                CollectedRatio = collectedRatio,
                ExistingCommissionId = existingCommissionId,
                Rates = rates,
            });
        }
    }
}