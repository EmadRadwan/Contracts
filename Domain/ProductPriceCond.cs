namespace Domain;

public class ProductPriceCond
{
    public string ProductPriceRuleId { get; set; } = null!;
    public string ProductPriceCondSeqId { get; set; } = null!;
    public string? InputParamEnumId { get; set; }
    public string? OperatorEnumId { get; set; }
    public string? CondValue { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Enumeration? InputParamEnum { get; set; }
    public Enumeration? OperatorEnum { get; set; }
    public ProductPriceRule ProductPriceRule { get; set; } = null!;
}