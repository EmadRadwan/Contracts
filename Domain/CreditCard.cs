namespace Domain;

public class CreditCard
{
    public string PaymentMethodId { get; set; } = null!;
    public string? CardType { get; set; }
    public string? CardNumber { get; set; }
    public string? ValidFromDate { get; set; }
    public string? ExpireDate { get; set; }
    public string? IssueNumber { get; set; }
    public string? CompanyNameOnCard { get; set; }
    public string? TitleOnCard { get; set; }
    public string? FirstNameOnCard { get; set; }
    public string? MiddleNameOnCard { get; set; }
    public string? LastNameOnCard { get; set; }
    public string? SuffixOnCard { get; set; }
    public string? ContactMechId { get; set; }
    public int? ConsecutiveFailedAuths { get; set; }
    public DateTime? LastFailedAuthDate { get; set; }
    public int? ConsecutiveFailedNsf { get; set; }
    public DateTime? LastFailedNsfDate { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ContactMech? ContactMech { get; set; }
    public PostalAddress? ContactMechNavigation { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = null!;
}