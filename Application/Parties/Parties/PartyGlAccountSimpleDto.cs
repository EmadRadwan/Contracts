namespace Application.Parties.Parties;

public class PartyGlAccountSimpleDto
{
    public string? GlAccountId { get; set; }
    public string? GlAccountTypeId { get; set; }
    public string? RoleTypeId { get; set; }
    public string? RoleDescription { get; set; }      // e.g. "BILL_FROM_VENDOR", "PARTNER"
    public string? AccountName { get; set; }
    public string? AccountNameArabic { get; set; }
    public string? AccountDescription { get; set; }
    public DateTime? CreatedStamp { get; set; }
}