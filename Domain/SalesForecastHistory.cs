namespace Domain;

public class SalesForecastHistory
{
    public string SalesForecastHistoryId { get; set; } = null!;
    public string? SalesForecastId { get; set; }
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
    public string? ChangeNote { get; set; }
    public string? ModifiedByUserLoginId { get; set; }
    public DateTime? ModifiedTimestamp { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Uom? CurrencyUom { get; set; }
    public CustomTimePeriod? CustomTimePeriod { get; set; }
    public Party? InternalParty { get; set; }
    public UserLogin? ModifiedByUserLogin { get; set; }
    public Party? OrganizationParty { get; set; }
    public SalesForecast? SalesForecast { get; set; }
}