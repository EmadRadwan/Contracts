namespace Domain;

public class CreditCardTypeGlAccount
{
    public string CardType { get; set; } = null!;
    public string OrganizationPartyId { get; set; } = null!;
    public string? GlAccountId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }
}