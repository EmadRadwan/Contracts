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

    // Every party slot is optional — the user may not know who the manager or the broker is when the
    // commission is first recorded. What is NOT optional is the pairing: a party and its percentage are
    // filled in together or left out together. A percentage without a party would compute an amount
    // nobody can be paid on approval, yet reports (SalesCommissionsDateRangeExcel) sum those amounts
    // blindly — so the totals would silently exceed the payments actually issued.
    public static string? ValidatePartyPercentPairing(SalesCommissionDto dto, bool isIndirect)
    {
        var slots = new List<(string? PartyId, decimal? Percent, string Label)>
        {
            (dto.SalesRepPartyId, dto.SalesRepPercent, "المندوب"),
            (dto.ManagerPartyId, dto.ManagerPercent, "المدير"),
            (dto.SalesRep2PartyId, dto.SalesRep2Percent, "المندوب الثاني"),
            (dto.Manager2PartyId, dto.Manager2Percent, "المدير الثاني"),
        };

        // External slots only exist for INDIRECT sales; the handlers null them out otherwise.
        if (isIndirect)
        {
            slots.Add((dto.ExternalCompanyPartyId, dto.ExternalCompanyPercent, "شركة الوسيط"));
            slots.Add((dto.ExternalSalesRepPartyId, dto.ExternalSalesRepPercent, "مندوب الوسيط"));
            slots.Add((dto.ExternalManagerPartyId, dto.ExternalManagerPercent, "مدير الوسيط"));
        }

        foreach (var (partyId, percent, label) in slots)
        {
            var hasParty = !string.IsNullOrEmpty(partyId);
            var hasPercent = (percent ?? 0m) > 0m;

            if (hasPercent && !hasParty)
                return $"تم إدخال نسبة {label} بدون تحديد الطرف — حدد {label} أو اجعل النسبة صفراً";

            if (hasParty && !hasPercent)
                return $"تم تحديد {label} بدون نسبة — أدخل نسبة {label} أو احذف الطرف";
        }

        return null;
    }

    // Belt-and-braces for the invariant ValidatePartyPercentPairing enforces: a slot with no party
    // earns nothing, so no amount is ever computed — and therefore stored — against a party that does
    // not exist. A no-op once the pairing check has passed.
    public static void ClearUnassignedPartyPercents(SalesCommissionDto dto)
    {
        if (string.IsNullOrEmpty(dto.SalesRepPartyId)) dto.SalesRepPercent = 0m;
        if (string.IsNullOrEmpty(dto.ManagerPartyId)) dto.ManagerPercent = 0m;
        if (string.IsNullOrEmpty(dto.SalesRep2PartyId)) dto.SalesRep2Percent = null;
        if (string.IsNullOrEmpty(dto.Manager2PartyId)) dto.Manager2Percent = null;
        if (string.IsNullOrEmpty(dto.ExternalCompanyPartyId)) dto.ExternalCompanyPercent = null;
        if (string.IsNullOrEmpty(dto.ExternalSalesRepPartyId)) dto.ExternalSalesRepPercent = null;
        if (string.IsNullOrEmpty(dto.ExternalManagerPartyId)) dto.ExternalManagerPercent = null;
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

        if (!isIndirect)
            return result;

        // The three external slots are independent: the broker company may be unknown while its rep is
        // already named, or vice versa. Each is computed on its own percentage — never gated on another.
        if (externalCompanyPercent.HasValue)
        {
            var extCompanyGross = salePrice * (externalCompanyPercent.Value / 100m) * commissionFactor;
            result.ExternalCompanyGrossAmount = extCompanyGross;

            // VAT exemption and withholding-tax exemption are independent tax statuses — a VAT-exempt
            // broker is still subject to WHT, and only HasWithholdingTaxExemption removes it. VAT, when
            // it applies, is embedded in the gross amount, so WHT is deducted from the VAT-exclusive
            // base; for a VAT-exempt broker nothing is embedded and the gross IS the base.
            var vatRate = vatPercent > 0 ? vatPercent : 14m;
            var baseAmount = hasVatExemption
                ? extCompanyGross
                : extCompanyGross * 100m / (100m + vatRate);

            result.ExternalCompanyNetAmount = (!hasWithholdingTaxExemption && withholdingTaxPercent > 0)
                ? extCompanyGross - baseAmount * (withholdingTaxPercent / 100m)
                : extCompanyGross;
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
