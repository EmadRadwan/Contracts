// Application/CostCenters/GetCostCenters.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Application.Core;

namespace Application.CostCenters;

public class GetCostCenters
{
    // Query مع فلتر اختياري: "in" = قبض فقط, "out" = صرف فقط, أو null = الكل
    public class Query : IRequest<Result<List<CostCenterDto>>>
    {
        public string Language { get; set; } = "ar";     // افتراضي عربي
        public string? Type { get; set; }                // "in" | "out" | null
    }

    public class Handler : IRequestHandler<Query, Result<List<CostCenterDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<CostCenterDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var language = request.Language?.ToLower() ?? "ar";
            var typeFilter = request.Type?.ToLower();

            IQueryable<Domain.CostCenter> query = _context.CostCenters;

            // فلترة حسب النوع
            if (typeFilter == "in")
                query = query.Where(cc => cc.IsOutPayment == "N");
            else if (typeFilter == "out")
                query = query.Where(cc => cc.IsOutPayment == "Y");
            // إذا null أو أي قيمة تانية → يجيب الكل

            var costCenters = await query
                .Select(cc => new CostCenterDto
                {
                    CostCenterId = cc.CostCenterId,
                    Description = language == "ar" ? cc.Description : cc.Description, // لو عايز إنجليزي أضف حقل DescriptionEnglish
                    IsOutPayment = cc.IsOutPayment == "Y"
                })
                .OrderBy(cc => cc.CostCenterId) // ترتيب حسب الـ ID الرقمي
                .ToListAsync(cancellationToken);

            return Result<List<CostCenterDto>>.Success(costCenters);
        }
    }
}

public class CostCenterDto
{
    public string CostCenterId { get; set; } = null!;
    public string Description { get; set; } = null!;
    public bool IsOutPayment { get; set; }   // true = صرف (out), false = قبض (in)
}