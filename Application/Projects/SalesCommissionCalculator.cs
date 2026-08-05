using Domain;

namespace Application.Projects;

// Shared validation + amount calculation for CreateSalesCommission and UpdateSalesCommission —
// keep the two handlers' business rules in sync by changing this file only.
public static class SalesCommissionCalculator
{
    public const decimal UpperThreshold = 0.10m;

    public static decimal GetLowerThreshold(bool isIndirect) => isIndirect ? 0.075m : 0.05m;

    // 0% below the lower threshold, 50% between the lower and upper threshold, 100% at/above the upper threshold.
    // This only scales the amount shown on a still-pending commission — creation/update is never blocked
    // by collection ratio, and approval always pays the full percentage-based amount regardless of it
    // (see CalculateFullAmounts / ApproveSalesCommission).
    public static decimal ComputeFactor(decimal collectedRatio, decimal lowerThreshold)
    {
        if (collectedRatio < lowerThreshold)
            return 0m;

        return collectedRatio >= UpperThreshold ? 1m : 0.5m;
    }

    public static string? ValidateRequiredParties(SalesCommissionDto dto, bool isIndirect)
    {
        if (string.IsNullOrEmpty(dto.SalesRepPartyId))
            return "يجب تحديد المندوب";

        if (string.IsNullOrEmpty(dto.ManagerPartyId))
            return "يجب تحديد المدير";

        if (dto.SalesRep2Percent.HasValue && string.IsNullOrEmpty(dto.SalesRep2PartyId))
            return "يجب تحديد المندوب الثاني عند إدخال نسبته";

        if (dto.Manager2Percent.HasValue && string.IsNullOrEmpty(dto.Manager2PartyId))
            return "يجب تحديد المدير الثاني عند إدخال نسبته";

        if (isIndirect && string.IsNullOrEmpty(dto.ExternalCompanyPartyId))
            return "يجب تحديد شركة الوسيط للبيع غير المباشر";

        return null;
    }

    public static string? ValidateAgainstConfiguredRate(SalesCommissionDto dto, ProjectCommissionRate? configuredRate, bool isIndirect)
    {
        if (configuredRate == null)
            return null;

        var totalRepPct = dto.SalesRepPercent + (dto.SalesRep2Percent ?? 0);
        if (totalRepPct > configuredRate.SalesRepPercent)
            return $"إجمالي نسبة المندوب ({totalRepPct:0.##}%) يتجاوز الحد المقرر للمشروع ({configuredRate.SalesRepPercent:0.##}%)";

        var totalMgrPct = dto.ManagerPercent + (dto.Manager2Percent ?? 0);
        if (totalMgrPct > configuredRate.ManagerPercent)
            return $"إجمالي نسبة المدير ({totalMgrPct:0.##}%) يتجاوز الحد المقرر للمشروع ({configuredRate.ManagerPercent:0.##}%)";

        if (isIndirect && configuredRate.ExternalCompanyPercent.HasValue && dto.ExternalCompanyPercent.HasValue
            && dto.ExternalCompanyPercent.Value > configuredRate.ExternalCompanyPercent.Value)
            return $"نسبة عمولة الوسيط ({dto.ExternalCompanyPercent:0.##}%) تتجاوز الحد المقرر للمشروع ({configuredRate.ExternalCompanyPercent:0.##}%)";

        if (isIndirect && configuredRate.ExternalSalesRepPercent.HasValue && dto.ExternalSalesRepPercent.HasValue
            && dto.ExternalSalesRepPercent.Value > configuredRate.ExternalSalesRepPercent.Value)
            return $"نسبة مندوب الوسيط ({dto.ExternalSalesRepPercent:0.##}%) تتجاوز الحد المقرر للمشروع ({configuredRate.ExternalSalesRepPercent:0.##}%)";

        if (isIndirect && configuredRate.ExternalManagerPercent.HasValue && dto.ExternalManagerPercent.HasValue
            && dto.ExternalManagerPercent.Value > configuredRate.ExternalManagerPercent.Value)
            return $"نسبة مدير الوسيط ({dto.ExternalManagerPercent:0.##}%) تتجاوز الحد المقرر للمشروع ({configuredRate.ExternalManagerPercent:0.##}%)";

        return null;
    }

    public class Amounts
    {
        public decimal SalesRepAmount { get; set; }
        public decimal ManagerAmount { get; set; }
        public decimal? SalesRep2Amount { get; set; }
        public decimal? Manager2Amount { get; set; }
        public decimal? ExternalCompanyGrossAmount { get; set; }
        public decimal? ExternalCompanyNetAmount { get; set; }
        public decimal? ExternalSalesRepAmount { get; set; }
        public decimal? ExternalSalesRepNetAmount { get; set; }
        public decimal? ExternalManagerAmount { get; set; }
        public decimal? ExternalManagerNetAmount { get; set; }
    }

    // isIndirect gates the external-broker fields — irrelevant fields stay null for DIRECT/PERSONAL sales
    public static Amounts CalculateAmounts(SalesCommissionDto dto, decimal salePrice, decimal commissionFactor, bool isIndirect) =>
        CalculateAmountsCore(
            dto.SalesRepPercent, dto.ManagerPercent,
            dto.SalesRep2Percent, dto.Manager2Percent,
            dto.ExternalCompanyPercent, dto.ExternalSalesRepPercent, dto.ExternalManagerPercent,
            dto.HasVatExemption, dto.VatPercent,
            dto.HasWithholdingTaxExemption, dto.WithholdingTaxPercent,
            dto.HasExternalSalesRepWithholdingTaxExemption, dto.HasExternalManagerWithholdingTaxExemption,
            salePrice, commissionFactor, isIndirect);

    // Full, collection-independent amounts for an already-persisted commission — used on approval,
    // where each party is paid their full percentage-based amount regardless of collection ratio.
    public static Amounts CalculateFullAmounts(Domain.SalesCommission commission, bool isIndirect) =>
        CalculateAmountsCore(
            commission.SalesRepPercent, commission.ManagerPercent,
            commission.SalesRep2Percent, commission.Manager2Percent,
            commission.ExternalCompanyPercent, commission.ExternalSalesRepPercent, commission.ExternalManagerPercent,
            commission.HasVatExemption, commission.VatPercent,
            commission.HasWithholdingTaxExemption, commission.WithholdingTaxPercent,
            commission.HasExternalSalesRepWithholdingTaxExemption, commission.HasExternalManagerWithholdingTaxExemption,
            commission.SalePrice, 1m, isIndirect);

    private static Amounts CalculateAmountsCore(
        decimal salesRepPercent, decimal managerPercent,
        decimal? salesRep2Percent, decimal? manager2Percent,
        decimal? externalCompanyPercent, decimal? externalSalesRepPercent, decimal? externalManagerPercent,
        bool hasVatExemption, decimal vatPercent,
        bool hasWithholdingTaxExemption, decimal withholdingTaxPercent,
        bool hasExternalSalesRepWithholdingTaxExemption, bool hasExternalManagerWithholdingTaxExemption,
        decimal salePrice, decimal commissionFactor, bool isIndirect)
    {
        var result = new Amounts
        {
            SalesRepAmount = salePrice * (salesRepPercent / 100m) * commissionFactor,
            ManagerAmount = salePrice * (managerPercent / 100m) * commissionFactor,
            SalesRep2Amount = salesRep2Percent.HasValue
                ? salePrice * (salesRep2Percent.Value / 100m) * commissionFactor
                : null,
            Manager2Amount = manager2Percent.HasValue
                ? salePrice * (manager2Percent.Value / 100m) * commissionFactor
                : null,
        };

        if (!isIndirect || !externalCompanyPercent.HasValue)
            return result;

        var extCompanyGross = salePrice * (externalCompanyPercent.Value / 100m) * commissionFactor;
        result.ExternalCompanyGrossAmount = extCompanyGross;

        if (!hasVatExemption)
        {
            // VAT is embedded in the gross amount: base = gross × 100/(100+VAT), then WHT is deducted from the base
            var vatRate = vatPercent > 0 ? vatPercent : 14m;
            var baseAmount = extCompanyGross * 100m / (100m + vatRate);

            result.ExternalCompanyNetAmount = (!hasWithholdingTaxExemption && withholdingTaxPercent > 0)
                ? extCompanyGross - baseAmount * (withholdingTaxPercent / 100m)
                : extCompanyGross;
        }
        else
        {
            result.ExternalCompanyNetAmount = extCompanyGross;
        }

        if (externalSalesRepPercent.HasValue)
        {
            var extSalesRepAmount = salePrice * (externalSalesRepPercent.Value / 100m) * commissionFactor;
            result.ExternalSalesRepAmount = extSalesRepAmount;
            result.ExternalSalesRepNetAmount = (!hasExternalSalesRepWithholdingTaxExemption && withholdingTaxPercent > 0)
                ? extSalesRepAmount - extSalesRepAmount * (withholdingTaxPercent / 100m)
                : extSalesRepAmount;
        }

        if (externalManagerPercent.HasValue)
        {
            var extManagerAmount = salePrice * (externalManagerPercent.Value / 100m) * commissionFactor;
            result.ExternalManagerAmount = extManagerAmount;
            result.ExternalManagerNetAmount = (!hasExternalManagerWithholdingTaxExemption && withholdingTaxPercent > 0)
                ? extManagerAmount - extManagerAmount * (withholdingTaxPercent / 100m)
                : extManagerAmount;
        }

        return result;
    }
}
