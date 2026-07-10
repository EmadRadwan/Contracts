using Application.Core;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Projects;

public class CreateSalesCommission
{
    public class Command : IRequest<Result<SalesCommissionDto>>
    {
        public SalesCommissionDto Dto { get; set; } = null!;
    }

    public class Handler : IRequestHandler<Command, Result<SalesCommissionDto>>
    {
        private readonly DataContext _context;
        private readonly ILogger<Handler> _logger;
        private readonly IUtilityService _utilityService;

        public Handler(DataContext context, ILogger<Handler> logger, IUtilityService utilityService)
        {
            _context = context;
            _logger = logger;
            _utilityService = utilityService;
        }

        public async Task<Result<SalesCommissionDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var dto = request.Dto;

            var sr = await _context.SalesRequests
                .Where(x => x.SalesRequestId == dto.SalesRequestId)
                .Join(_context.Products,
                    s => s.ProductId,
                    p => p.ProductId,
                    (s, p) => new { s, p })
                .FirstOrDefaultAsync(cancellationToken);

            if (sr == null)
                return Result<SalesCommissionDto>.Failure("Sales request not found");

            if (sr.s.StatusId != "SALES_REQUEST_APPROVED")
                return Result<SalesCommissionDto>.Failure("Commission can only be created for approved sales requests");

            var existing = await _context.SalesCommissions
                .FirstOrDefaultAsync(x => x.SalesRequestId == dto.SalesRequestId, cancellationToken);
            if (existing != null)
                return Result<SalesCommissionDto>.Failure("A commission record already exists for this sales request");

            var projectId = sr.p.ProjectId;
            var isIndirect = dto.SaleTypeId == "COMM_SALE_INDIRECT";

            if (string.IsNullOrEmpty(dto.SalesRepPartyId))
                return Result<SalesCommissionDto>.Failure("يجب تحديد المندوب");

            if (string.IsNullOrEmpty(dto.ManagerPartyId))
                return Result<SalesCommissionDto>.Failure("يجب تحديد المدير");

            if (dto.SalesRep2Percent.HasValue && string.IsNullOrEmpty(dto.SalesRep2PartyId))
                return Result<SalesCommissionDto>.Failure("يجب تحديد المندوب الثاني عند إدخال نسبته");

            if (dto.Manager2Percent.HasValue && string.IsNullOrEmpty(dto.Manager2PartyId))
                return Result<SalesCommissionDto>.Failure("يجب تحديد المدير الثاني عند إدخال نسبته");

            if (isIndirect && string.IsNullOrEmpty(dto.ExternalCompanyPartyId))
                return Result<SalesCommissionDto>.Failure("يجب تحديد شركة الوسيط للبيع غير المباشر");

            // Derive salePrice and collectedAmount from the database — never trust client input
            var salePrice = sr.s.TotalPrice ?? 0m;

            var collectedAmount = await _context.Payments
                .Where(p => p.SalesRequestId == dto.SalesRequestId
                         && p.Amount > 0
                         && (p.PaymentTypeId == "RECEIPT_ADVANCE_PAYMENT"
                             || p.PaymentTypeId == "RECEIPT_DUE_INSTALLMENT")
                         && p.StatusId == "PMNT_RECEIVED")
                .SumAsync(p => (decimal?)p.Amount, cancellationToken) ?? 0m;

            // Threshold rules
            var ratio = salePrice > 0 ? collectedAmount / salePrice : 0m;
            var lowerThreshold = isIndirect ? 0.075m : 0.05m;
            const decimal upperThreshold = 0.10m;

            if (ratio < lowerThreshold)
            {
                var pct = (lowerThreshold * 100).ToString("0.#");
                return Result<SalesCommissionDto>.Failure(
                    $"المبلغ المحصل يجب أن يصل إلى {pct}% من سعر البيع قبل إنشاء العمولة");
            }

            // 50% factor when collected ratio is between lower threshold and 10%
            var commissionFactor = ratio >= upperThreshold ? 1m : 0.5m;

            // Validate submitted percentages against the configured project rate
            var configuredRate = projectId != null
                ? await _context.ProjectCommissionRates
                    .FirstOrDefaultAsync(x => x.ProjectId == projectId && x.SaleTypeId == dto.SaleTypeId, cancellationToken)
                : null;

            if (configuredRate != null)
            {
                var totalRepPct = dto.SalesRepPercent + (dto.SalesRep2Percent ?? 0);
                if (totalRepPct > configuredRate.SalesRepPercent)
                    return Result<SalesCommissionDto>.Failure(
                        $"إجمالي نسبة المندوب ({totalRepPct:0.##}%) يتجاوز الحد المقرر للمشروع ({configuredRate.SalesRepPercent:0.##}%)");

                var totalMgrPct = dto.ManagerPercent + (dto.Manager2Percent ?? 0);
                if (totalMgrPct > configuredRate.ManagerPercent)
                    return Result<SalesCommissionDto>.Failure(
                        $"إجمالي نسبة المدير ({totalMgrPct:0.##}%) يتجاوز الحد المقرر للمشروع ({configuredRate.ManagerPercent:0.##}%)");

                if (isIndirect && configuredRate.ExternalCompanyPercent.HasValue && dto.ExternalCompanyPercent.HasValue
                    && dto.ExternalCompanyPercent.Value > configuredRate.ExternalCompanyPercent.Value)
                    return Result<SalesCommissionDto>.Failure(
                        $"نسبة عمولة الوسيط ({dto.ExternalCompanyPercent:0.##}%) تتجاوز الحد المقرر للمشروع ({configuredRate.ExternalCompanyPercent:0.##}%)");
            }
            var stamp = DateTime.UtcNow;
            var newId = await _utilityService.GetNextSequence("SalesCommission");

            // Calculate amounts — apply commissionFactor (1.0 = full, 0.5 = partial)
            decimal salesRepAmount = salePrice * (dto.SalesRepPercent / 100m) * commissionFactor;
            decimal managerAmount = salePrice * (dto.ManagerPercent / 100m) * commissionFactor;
            decimal? salesRep2Amount = dto.SalesRep2Percent.HasValue
                ? salePrice * (dto.SalesRep2Percent.Value / 100m) * commissionFactor : null;
            decimal? manager2Amount = dto.Manager2Percent.HasValue
                ? salePrice * (dto.Manager2Percent.Value / 100m) * commissionFactor : null;

            decimal? extCompanyGross = null;
            decimal? extCompanyNet = null;
            decimal? extSalesRepAmount = null;
            decimal? extSalesRepNetAmount = null;
            decimal? extManagerAmount = null;
            decimal? extManagerNetAmount = null;

            if (isIndirect && dto.ExternalCompanyPercent.HasValue)
            {
                extCompanyGross = salePrice * (dto.ExternalCompanyPercent.Value / 100m) * commissionFactor;

                if (!dto.HasVatExemption)
                {
                    // VAT is embedded: base = gross × 100/114, then WHT on base
                    var vatRate = dto.VatPercent > 0 ? dto.VatPercent : 14m;
                    var vatDivisor = 100m + vatRate;
                    var baseAmount = extCompanyGross.Value * 100m / vatDivisor;
                    var vatAmount = extCompanyGross.Value - baseAmount;

                    if (!dto.HasWithholdingTaxExemption && dto.WithholdingTaxPercent > 0)
                    {
                        var whtAmount = baseAmount * (dto.WithholdingTaxPercent / 100m);
                        extCompanyNet = extCompanyGross.Value - whtAmount;
                    }
                    else
                    {
                        extCompanyNet = extCompanyGross.Value;
                    }
                }
                else
                {
                    extCompanyNet = extCompanyGross;
                }

                if (dto.ExternalSalesRepPercent.HasValue)
                {
                    extSalesRepAmount = salePrice * (dto.ExternalSalesRepPercent.Value / 100m) * commissionFactor;
                    extSalesRepNetAmount = (!dto.HasExternalSalesRepWithholdingTaxExemption && dto.WithholdingTaxPercent > 0)
                        ? extSalesRepAmount.Value - extSalesRepAmount.Value * (dto.WithholdingTaxPercent / 100m)
                        : extSalesRepAmount;
                }

                if (dto.ExternalManagerPercent.HasValue)
                {
                    extManagerAmount = salePrice * (dto.ExternalManagerPercent.Value / 100m) * commissionFactor;
                    extManagerNetAmount = (!dto.HasExternalManagerWithholdingTaxExemption && dto.WithholdingTaxPercent > 0)
                        ? extManagerAmount.Value - extManagerAmount.Value * (dto.WithholdingTaxPercent / 100m)
                        : extManagerAmount;
                }
            }

            var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var commission = new Domain.SalesCommission
                {
                    SalesCommissionId = newId,
                    SalesRequestId = dto.SalesRequestId,
                    SaleTypeId = dto.SaleTypeId,
                    StatusId = "COMMISSION_PENDING",
                    CommissionDate = dto.CommissionDate ?? stamp,
                    ProjectId = projectId,
                    SalePrice = salePrice,
                    CollectedAmount = collectedAmount,
                    SalesRepPartyId = dto.SalesRepPartyId,
                    SalesRepPercent = dto.SalesRepPercent,
                    SalesRepAmount = salesRepAmount,
                    SalesRepNetAmount = salesRepAmount,
                    ManagerPartyId = dto.ManagerPartyId,
                    ManagerPercent = dto.ManagerPercent,
                    ManagerAmount = managerAmount,
                    ManagerNetAmount = managerAmount,
                    SalesRep2PartyId = dto.SalesRep2PartyId,
                    SalesRep2Percent = dto.SalesRep2Percent,
                    SalesRep2Amount = salesRep2Amount,
                    SalesRep2NetAmount = salesRep2Amount,
                    Manager2PartyId = dto.Manager2PartyId,
                    Manager2Percent = dto.Manager2Percent,
                    Manager2Amount = manager2Amount,
                    Manager2NetAmount = manager2Amount,
                    ExternalCompanyPartyId = isIndirect ? dto.ExternalCompanyPartyId : null,
                    ExternalCompanyPercent = isIndirect ? dto.ExternalCompanyPercent : null,
                    ExternalCompanyGrossAmount = isIndirect ? extCompanyGross : null,
                    ExternalCompanyNetAmount = isIndirect ? extCompanyNet : null,
                    ExternalSalesRepPartyId = isIndirect ? dto.ExternalSalesRepPartyId : null,
                    ExternalSalesRepPercent = isIndirect ? dto.ExternalSalesRepPercent : null,
                    ExternalSalesRepAmount = isIndirect ? extSalesRepAmount : null,
                    ExternalSalesRepNetAmount = isIndirect ? extSalesRepNetAmount : null,
                    HasExternalSalesRepWithholdingTaxExemption = isIndirect && dto.HasExternalSalesRepWithholdingTaxExemption,
                    ExternalSalesRepNationalId = isIndirect ? dto.ExternalSalesRepNationalId : null,
                    ExternalManagerPartyId = isIndirect ? dto.ExternalManagerPartyId : null,
                    ExternalManagerPercent = isIndirect ? dto.ExternalManagerPercent : null,
                    ExternalManagerAmount = isIndirect ? extManagerAmount : null,
                    ExternalManagerNetAmount = isIndirect ? extManagerNetAmount : null,
                    HasExternalManagerWithholdingTaxExemption = isIndirect && dto.HasExternalManagerWithholdingTaxExemption,
                    ExternalManagerNationalId = isIndirect ? dto.ExternalManagerNationalId : null,
                    HasVatExemption = dto.HasVatExemption,
                    HasWithholdingTaxExemption = dto.HasWithholdingTaxExemption,
                    VatPercent = dto.VatPercent,
                    WithholdingTaxPercent = dto.WithholdingTaxPercent,
                    Notes = dto.Notes,
                    CreatedStamp = stamp,
                    LastUpdatedStamp = stamp
                };

                _context.SalesCommissions.Add(commission);
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                dto.SalesCommissionId = newId;
                dto.StatusId = "COMMISSION_PENDING";
                dto.SalesRepAmount = commission.SalesRepAmount;
                dto.SalesRepNetAmount = commission.SalesRepNetAmount;
                dto.ManagerAmount = commission.ManagerAmount;
                dto.ManagerNetAmount = commission.ManagerNetAmount;
                dto.SalesRep2Amount = commission.SalesRep2Amount;
                dto.SalesRep2NetAmount = commission.SalesRep2NetAmount;
                dto.Manager2Amount = commission.Manager2Amount;
                dto.Manager2NetAmount = commission.Manager2NetAmount;
                dto.ExternalCompanyGrossAmount = commission.ExternalCompanyGrossAmount;
                dto.ExternalCompanyNetAmount = commission.ExternalCompanyNetAmount;
                dto.ExternalSalesRepAmount = commission.ExternalSalesRepAmount;
                dto.ExternalSalesRepNetAmount = commission.ExternalSalesRepNetAmount;
                dto.ExternalManagerAmount = commission.ExternalManagerAmount;
                dto.ExternalManagerNetAmount = commission.ExternalManagerNetAmount;
                dto.HasExternalSalesRepWithholdingTaxExemption = commission.HasExternalSalesRepWithholdingTaxExemption;
                dto.HasExternalManagerWithholdingTaxExemption = commission.HasExternalManagerWithholdingTaxExemption;

                return Result<SalesCommissionDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create sales commission");
                await transaction.RollbackAsync(cancellationToken);
                return Result<SalesCommissionDto>.Failure("Failed to create sales commission");
            }
        }
    }
}
