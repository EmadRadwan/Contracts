namespace Domain;

public class SalesForecast
{
    public SalesForecast()
    {
        InverseParentSalesForecast = new HashSet<SalesForecast>();
        SalesForecastDetails = new HashSet<SalesForecastDetail>();
        SalesForecastHistories = new HashSet<SalesForecastHistory>();
    }

    public string SalesForecastId { get; set; } = null!;
    public string? ParentSalesForecastId { get; set; }
    public string? OrganizationPartyId { get; set; }
    public string? InternalPartyId { get; set; }
    public string? CustomTimePeriodId { get; set; }
    public string? CurrencyUomId { get; set; }
    public decimal? QuotaAmount { get; set; }
    public decimal? ForecastAmount { get; set; }
    public decimal? BestCaseAmount { get; set; }
    public decimal? ClosedAmount { get; set; }
    public decimal? PercentOfQuotaForecast { get; set; }
    public decimal? PercentOfQuotaClosed { get; set; }
    public decimal? PipelineAmount { get; set; }
    public string? CreatedByUserLoginId { get; set; }
    public string? ModifiedByUserLoginId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public UserLogin? CreatedByUserLogin { get; set; }
    public Uom? CurrencyUom { get; set; }
    public CustomTimePeriod? CustomTimePeriod { get; set; }
    public Party? InternalParty { get; set; }
    public UserLogin? ModifiedByUserLogin { get; set; }
    public Party? OrganizationParty { get; set; }
    public SalesForecast? ParentSalesForecast { get; set; }
    public ICollection<SalesForecast> InverseParentSalesForecast { get; set; }
    public ICollection<SalesForecastDetail> SalesForecastDetails { get; set; }
    public ICollection<SalesForecastHistory> SalesForecastHistories { get; set; }
}