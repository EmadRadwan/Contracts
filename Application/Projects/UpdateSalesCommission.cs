using Application.Core;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Projects;

public class UpdateSalesCommission
{
    public class Command : IRequest<Result<SalesCommissionDto>>
    {
        public SalesCommissionDto Dto { get; set; } = null!;
    }

    public class Handler : IRequestHandler<Command, Result<SalesCommissionDto>>
    {
        private readonly DataContext _context;
        private readonly ILogger<Handler> _logger;

        public Handler(DataContext context, ILogger<Handler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result<SalesCommissionDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var dto = request.Dto;

            var commission = await _context.SalesCommissions
                .FirstOrDefaultAsync(x => x.SalesCommissionId == dto.SalesCommissionId, cancellationToken);

            if (commission == null)
                return Result<SalesCommissionDto>.Failure("Commission record not found");

            if (commission.StatusId != "COMMISSION_PENDING")
                return Result<SalesCommissionDto>.Failure("Only pending commissions can be updated");

            var sr = await _context.SalesRequests
                .Where(x => x.SalesRequestId == commission.SalesRequestId)
                .Join(_context.Products,
                    s => s.ProductId,
                    p => p.ProductId,
                    (s, p) => new { s, p })
                .FirstOrDefaultAsync(cancellationToken);

            if (sr == null)
                return Result<SalesCommissionDto>.Failure("Sales request not found");

            var isIndirect = dto.SaleTypeId == "COMM_SALE_INDIRECT";
            var projectId = commission.ProjectId ?? sr.p.ProjectId;
            var salePrice = sr.s.TotalPrice ?? 0m;

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

            var collectedAmount = await _context.Payments
                .Where(p => p.SalesRequestId == commission.SalesRequestId
                         && p.Amount > 0
                         && (p.PaymentTypeId == "RECEIPT_ADVANCE_PAYMENT"
                             || p.PaymentTypeId == "RECEIPT_DUE_INSTALLMENT")
                         && p.StatusId == "PMNT_RECEIVED")
                .SumAsync(p => (decimal?)p.Amount, cancellationToken) ?? 0m;

            var ratio = salePrice > 0 ? collectedAmount / salePrice : 0m;
            var lowerThreshold = isIndirect ? 0.075m : 0.05m;
            const decimal upperThreshold = 0.10m;

            if (ratio < lowerThreshold)
            {
                var pct = (lowerThreshold * 100).ToString("0.#");
                return Result<SalesCommissionDto>.Failure(
                    $"المبلغ المحصل يجب أن يصل إلى {pct}% من سعر البيع قبل تعديل العمولة");
            }

            var commissionFactor = ratio >= upperThreshold ? 1m : 0.5m;

            decimal salesRepAmount = salePrice * (dto.SalesRepPercent / 100m) * commissionFactor;
            decimal managerAmount = salePrice * (dto.ManagerPercent / 100m) * commissionFactor;
            decimal? salesRep2Amount = dto.SalesRep2Percent.HasValue
                ? salePrice * (dto.SalesRep2Percent.Value / 100m) * commissionFactor : null;
            decimal? manager2Amount = dto.Manager2Percent.HasValue
                ? salePrice * (dto.Manager2Percent.Value / 100m) * commissionFactor : null;

            decimal? extCompanyGross = null, extCompanyNet = null;
            decimal? extSalesRepAmount = null, extSalesRepNetAmount = null;
            decimal? extManagerAmount = null, extManagerNetAmount = null;

            if (isIndirect && dto.ExternalCompanyPercent.HasValue)
            {
                extCompanyGross = salePrice * (dto.ExternalCompanyPercent.Value / 100m) * commissionFactor;

                if (!dto.HasVatExemption)
                {
                    var vatRate = dto.VatPercent > 0 ? dto.VatPercent : 14m;
                    var baseAmount = extCompanyGross.Value * 100m / (100m + vatRate);

                    extCompanyNet = (!dto.HasWithholdingTaxExemption && dto.WithholdingTaxPercent > 0)
                        ? extCompanyGross.Value - baseAmount * (dto.WithholdingTaxPercent / 100m)
                        : extCompanyGross.Value;
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

            var stamp = DateTime.UtcNow;
            try
            {
                commission.SaleTypeId = dto.SaleTypeId;
                commission.SalePrice = salePrice;
                commission.CollectedAmount = collectedAmount;
                commission.SalesRepPartyId = dto.SalesRepPartyId;
                commission.SalesRepPercent = dto.SalesRepPercent;
                commission.SalesRepAmount = salesRepAmount;
                commission.SalesRepNetAmount = salesRepAmount;
                commission.ManagerPartyId = dto.ManagerPartyId;
                commission.ManagerPercent = dto.ManagerPercent;
                commission.ManagerAmount = managerAmount;
                commission.ManagerNetAmount = managerAmount;
                commission.SalesRep2PartyId = dto.SalesRep2PartyId;
                commission.SalesRep2Percent = dto.SalesRep2Percent;
                commission.SalesRep2Amount = salesRep2Amount;
                commission.SalesRep2NetAmount = salesRep2Amount;
                commission.Manager2PartyId = dto.Manager2PartyId;
                commission.Manager2Percent = dto.Manager2Percent;
                commission.Manager2Amount = manager2Amount;
                commission.Manager2NetAmount = manager2Amount;
                commission.ExternalCompanyPartyId = isIndirect ? dto.ExternalCompanyPartyId : null;
                commission.ExternalCompanyPercent = isIndirect ? dto.ExternalCompanyPercent : null;
                commission.ExternalCompanyGrossAmount = isIndirect ? extCompanyGross : null;
                commission.ExternalCompanyNetAmount = isIndirect ? extCompanyNet : null;
                commission.ExternalSalesRepPartyId = isIndirect ? dto.ExternalSalesRepPartyId : null;
                commission.ExternalSalesRepPercent = isIndirect ? dto.ExternalSalesRepPercent : null;
                commission.ExternalSalesRepAmount = isIndirect ? extSalesRepAmount : null;
                commission.ExternalSalesRepNetAmount = isIndirect ? extSalesRepNetAmount : null;
                commission.HasExternalSalesRepWithholdingTaxExemption = isIndirect && dto.HasExternalSalesRepWithholdingTaxExemption;
                commission.ExternalSalesRepNationalId = isIndirect ? dto.ExternalSalesRepNationalId : null;
                commission.ExternalManagerPartyId = isIndirect ? dto.ExternalManagerPartyId : null;
                commission.ExternalManagerPercent = isIndirect ? dto.ExternalManagerPercent : null;
                commission.ExternalManagerAmount = isIndirect ? extManagerAmount : null;
                commission.ExternalManagerNetAmount = isIndirect ? extManagerNetAmount : null;
                commission.HasExternalManagerWithholdingTaxExemption = isIndirect && dto.HasExternalManagerWithholdingTaxExemption;
                commission.ExternalManagerNationalId = isIndirect ? dto.ExternalManagerNationalId : null;
                commission.HasVatExemption = dto.HasVatExemption;
                commission.HasWithholdingTaxExemption = dto.HasWithholdingTaxExemption;
                commission.VatPercent = dto.VatPercent;
                commission.WithholdingTaxPercent = dto.WithholdingTaxPercent;
                commission.Notes = dto.Notes;
                commission.LastUpdatedStamp = stamp;

                await _context.SaveChangesAsync(cancellationToken);

                dto.SalesCommissionId = commission.SalesCommissionId;
                dto.StatusId = commission.StatusId;
                dto.SalePrice = salePrice;
                dto.CollectedAmount = collectedAmount;
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
                _logger.LogError(ex, "Failed to update sales commission");
                return Result<SalesCommissionDto>.Failure("Failed to update sales commission");
            }
        }
    }
}
