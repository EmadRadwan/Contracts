using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class AgreementProductAppl
{
    public string AgreementId { get; set; } = null!;
    public string AgreementItemSeqId { get; set; } = null!;
    public string ProductId { get; set; } = null!;
    public decimal? Price { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public AgreementItem AgreementI { get; set; } = null!;
    public Product Product { get; set; } = null!;
}