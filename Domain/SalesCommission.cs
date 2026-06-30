using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class SalesCommission
{
    public string SalesCommissionId { get; set; } = null!;

    public string SalesRequestId { get; set; } = null!;

    // COMM_SALE_DIRECT | COMM_SALE_PERSONAL | COMM_SALE_INDIRECT
    public string SaleTypeId { get; set; } = null!;

    // COMMISSION_PENDING | COMMISSION_APPROVED | COMMISSION_PAID
    public string? StatusId { get; set; }

    public DateTime? CommissionDate { get; set; }

    // -----------------------------------------------------------------
    // Sale snapshot (denormalized from SalesRequest / Product at creation)
    // -----------------------------------------------------------------
    public string? ProjectId { get; set; }
    public decimal SalePrice { get; set; }
    public decimal? CollectedAmount { get; set; }

    // -----------------------------------------------------------------
    // Internal sales rep — present for all sale types
    // -----------------------------------------------------------------
    public string? SalesRepPartyId { get; set; }
    public decimal SalesRepPercent { get; set; }
    public decimal? SalesRepAmount { get; set; }
    public decimal? SalesRepNetAmount { get; set; }

    // -----------------------------------------------------------------
    // Internal manager — present for all sale types
    // -----------------------------------------------------------------
    public string? ManagerPartyId { get; set; }
    public decimal ManagerPercent { get; set; }
    public decimal? ManagerAmount { get; set; }
    public decimal? ManagerNetAmount { get; set; }

    // -----------------------------------------------------------------
    // Second internal sales rep — DIRECT / PERSONAL split only
    // -----------------------------------------------------------------
    public string? SalesRep2PartyId { get; set; }
    public decimal? SalesRep2Percent { get; set; }
    public decimal? SalesRep2Amount { get; set; }
    public decimal? SalesRep2NetAmount { get; set; }

    // -----------------------------------------------------------------
    // Second internal manager — DIRECT / PERSONAL split only
    // -----------------------------------------------------------------
    public string? Manager2PartyId { get; set; }
    public decimal? Manager2Percent { get; set; }
    public decimal? Manager2Amount { get; set; }
    public decimal? Manager2NetAmount { get; set; }

    // -----------------------------------------------------------------
    // External broker company — INDIRECT only
    // -----------------------------------------------------------------
    public string? ExternalCompanyPartyId { get; set; }
    public decimal? ExternalCompanyPercent { get; set; }
    public decimal? ExternalCompanyGrossAmount { get; set; }
    public decimal? ExternalCompanyNetAmount { get; set; }

    // -----------------------------------------------------------------
    // External broker's sales rep — INDIRECT only
    // -----------------------------------------------------------------
    public string? ExternalSalesRepPartyId { get; set; }
    public decimal? ExternalSalesRepPercent { get; set; }
    public decimal? ExternalSalesRepAmount { get; set; }
    public decimal? ExternalSalesRepNetAmount { get; set; }
    public bool HasExternalSalesRepWithholdingTaxExemption { get; set; }
    public string? ExternalSalesRepNationalId { get; set; }

    // -----------------------------------------------------------------
    // External broker's manager — INDIRECT only
    // -----------------------------------------------------------------
    public string? ExternalManagerPartyId { get; set; }
    public decimal? ExternalManagerPercent { get; set; }
    public decimal? ExternalManagerAmount { get; set; }
    public decimal? ExternalManagerNetAmount { get; set; }
    public bool HasExternalManagerWithholdingTaxExemption { get; set; }
    public string? ExternalManagerNationalId { get; set; }

    // -----------------------------------------------------------------
    // Tax configuration (applies to external company invoice)
    // HasVatExemption        = true  → skip the ×100/114 split; pay gross as-is
    // HasWithholdingTaxExemption = true  → do not deduct withholding from net
    // -----------------------------------------------------------------
    public bool HasVatExemption { get; set; }
    public bool HasWithholdingTaxExemption { get; set; }
    public decimal VatPercent { get; set; }
    public decimal WithholdingTaxPercent { get; set; }

    public string? Notes { get; set; }

    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }

    // -----------------------------------------------------------------
    // Navigation
    // -----------------------------------------------------------------
    public virtual SalesRequest SalesRequest { get; set; } = null!;
    public virtual WorkEffort? Project { get; set; }
    public virtual Party? SalesRepParty { get; set; }
    public virtual Party? ManagerParty { get; set; }
    public virtual Party? SalesRep2Party { get; set; }
    public virtual Party? Manager2Party { get; set; }
    public virtual Party? ExternalCompanyParty { get; set; }
    public virtual Party? ExternalSalesRepParty { get; set; }
    public virtual Party? ExternalManagerParty { get; set; }
}
