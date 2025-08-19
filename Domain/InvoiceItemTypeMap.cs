using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class InvoiceItemTypeMap
{
    public string InvoiceItemMapKey { get; set; } = null!;
    public string InvoiceTypeId { get; set; } = null!;
    public string? InvoiceItemTypeId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public InvoiceItemType? InvoiceItemType { get; set; }
    public InvoiceType InvoiceType { get; set; } = null!;
}