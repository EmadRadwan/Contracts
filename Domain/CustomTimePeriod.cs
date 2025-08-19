using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class CustomTimePeriod
{
    public CustomTimePeriod()
    {
        Budgets = new HashSet<Budget>();
        GlAccountHistories = new HashSet<GlAccountHistory>();
        InverseParentPeriod = new HashSet<CustomTimePeriod>();
        SalesForecastHistories = new HashSet<SalesForecastHistory>();
        SalesForecasts = new HashSet<SalesForecast>();
    }

    public string CustomTimePeriodId { get; set; } = null!;
    public string? ParentPeriodId { get; set; }
    public string? PeriodTypeId { get; set; }
    public int? PeriodNum { get; set; }
    public string? PeriodName { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public string? IsClosed { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }
    public string? OrganizationPartyId { get; set; }

    public Party? OrganizationParty { get; set; }
    public CustomTimePeriod? ParentPeriod { get; set; }
    public PeriodType? PeriodType { get; set; }
    public ICollection<Budget> Budgets { get; set; }
    public ICollection<GlAccountHistory> GlAccountHistories { get; set; }
    public ICollection<CustomTimePeriod> InverseParentPeriod { get; set; }
    public ICollection<SalesForecastHistory> SalesForecastHistories { get; set; }
    public ICollection<SalesForecast> SalesForecasts { get; set; }
}