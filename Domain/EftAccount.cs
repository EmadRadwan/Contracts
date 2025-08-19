namespace Domain;

public class EftAccount
{
    public string PaymentMethodId { get; set; } = null!;
    public string? BankName { get; set; }
    public string? RoutingNumber { get; set; }
    public string? AccountType { get; set; }
    public string? AccountNumber { get; set; }
    public string? NameOnAccount { get; set; }
    public string? CompanyNameOnAccount { get; set; }
    public string? ContactMechId { get; set; }
    public int? YearsAtBank { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ContactMech? ContactMech { get; set; }
    public PostalAddress? ContactMechNavigation { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = null!;
}