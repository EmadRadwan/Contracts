namespace Domain;

public class GiftCard
{
    public string PaymentMethodId { get; set; } = null!;
    public string? CardNumber { get; set; }
    public string? PinNumber { get; set; }
    public string? ExpireDate { get; set; }
    public string? ContactMechId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ContactMech? ContactMech { get; set; }
    public PostalAddress? ContactMechNavigation { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = null!;
}