namespace Domain;

public class ProductStoreFinActSetting
{
    public string ProductStoreId { get; set; } = null!;
    public string FinAccountTypeId { get; set; } = null!;
    public string? RequirePinCode { get; set; }
    public string? ValidateGCFinAcct { get; set; }
    public int? AccountCodeLength { get; set; }
    public int? PinCodeLength { get; set; }
    public int? AccountValidDays { get; set; }
    public int? AuthValidDays { get; set; }
    public string? PurchaseSurveyId { get; set; }
    public string? PurchSurveySendTo { get; set; }
    public string? PurchSurveyCopyMe { get; set; }
    public string? AllowAuthToNegative { get; set; }
    public decimal? MinBalance { get; set; }
    public decimal? ReplenishThreshold { get; set; }
    public string? ReplenishMethodEnumId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public FinAccountType FinAccountType { get; set; } = null!;
    public ProductStore ProductStore { get; set; } = null!;
    public Survey? PurchaseSurvey { get; set; }
    public Enumeration? ReplenishMethodEnum { get; set; }
}