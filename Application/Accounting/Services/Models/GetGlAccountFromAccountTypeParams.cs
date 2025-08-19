namespace Application.Accounting.Services.Models;

public class GetGlAccountFromAccountTypeParams
{
    public string? OrganizationPartyId { get; set; }
    public string? AcctgTransTypeId { get; set; }
    public string? GlAccountTypeId { get; set; }
    public string? DebitCreditFlag { get; set; }
    public string? ProductId { get; set; }
    public string? PartyId { get; set; }
    public string? RoleTypeId { get; set; }
    public string? InvoiceId { get; set; }
    public string? PaymentId { get; set; }
    public string? FixedAssetId { get; set; }
    public string? InvoiceItemTypeId { get; set; }
}