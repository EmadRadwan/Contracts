namespace Application.Accounting.OrganizationGlSettings;

public class GetPartyGlAccountDto
{
    public string PartyId { get; set; }
    public string PartyDescription { get; set; }
    public string GlAccountTypeDescription { get; set; }
    public string GlAccountId { get; set; }
    public string GlAccountName { get; set; }
    public string RoleTypeId { get; set; }
    public string RoleDescription { get; set; }
    public string ParentGlAccountId { get; set; }
    public string ParentGlAccountName { get; set; }
}