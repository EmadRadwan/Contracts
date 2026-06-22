namespace Application.Projects;

public class SalesCommissionDto
{
    public string? SalesCommissionId { get; set; }
    public string SalesRequestId { get; set; } = null!;
    public string SaleTypeId { get; set; } = null!;
    public string? StatusId { get; set; }
    public DateTime? CommissionDate { get; set; }

    public string? ProjectId { get; set; }
    public decimal SalePrice { get; set; }
    public decimal? CollectedAmount { get; set; }

    // Internal sales rep 1
    public string? SalesRepPartyId { get; set; }
    public string? SalesRepName { get; set; }
    public decimal SalesRepPercent { get; set; }
    public decimal? SalesRepAmount { get; set; }
    public decimal? SalesRepNetAmount { get; set; }

    // Internal manager 1
    public string? ManagerPartyId { get; set; }
    public string? ManagerName { get; set; }
    public decimal ManagerPercent { get; set; }
    public decimal? ManagerAmount { get; set; }
    public decimal? ManagerNetAmount { get; set; }

    // Internal sales rep 2 (split)
    public string? SalesRep2PartyId { get; set; }
    public string? SalesRep2Name { get; set; }
    public decimal? SalesRep2Percent { get; set; }
    public decimal? SalesRep2Amount { get; set; }
    public decimal? SalesRep2NetAmount { get; set; }

    // Internal manager 2 (split)
    public string? Manager2PartyId { get; set; }
    public string? Manager2Name { get; set; }
    public decimal? Manager2Percent { get; set; }
    public decimal? Manager2Amount { get; set; }
    public decimal? Manager2NetAmount { get; set; }

    // External broker company — INDIRECT only
    public string? ExternalCompanyPartyId { get; set; }
    public string? ExternalCompanyName { get; set; }
    public decimal? ExternalCompanyPercent { get; set; }
    public decimal? ExternalCompanyGrossAmount { get; set; }
    public decimal? ExternalCompanyNetAmount { get; set; }

    // External broker's sales rep — INDIRECT only
    public string? ExternalSalesRepPartyId { get; set; }
    public string? ExternalSalesRepName { get; set; }
    public decimal? ExternalSalesRepPercent { get; set; }
    public decimal? ExternalSalesRepAmount { get; set; }
    public decimal? ExternalSalesRepNetAmount { get; set; }

    // External broker's manager — INDIRECT only
    public string? ExternalManagerPartyId { get; set; }
    public string? ExternalManagerName { get; set; }
    public decimal? ExternalManagerPercent { get; set; }
    public decimal? ExternalManagerAmount { get; set; }
    public decimal? ExternalManagerNetAmount { get; set; }

    // Tax flags
    public bool HasVatExemption { get; set; }
    public bool HasWithholdingTaxExemption { get; set; }
    public decimal VatPercent { get; set; }
    public decimal WithholdingTaxPercent { get; set; }

    public string? Notes { get; set; }
}
