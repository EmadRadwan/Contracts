namespace Application.Shipments.OrganizationGlSettings;

public class GetFixedAssetTypeGlAccountDto
{
    public string FixedAssetTypeId { get; set; }
    public string FixedAssetId { get; set; }
    public string OrganizationPartyId { get; set; }
    public string OrganizationPartyName { get; set; }
    public string AssetGlAccountId { get; set; }
    public string AssetGlAccountName { get; set; }
    public string AccDepGlAccountId { get; set; }
    public string AccDepGlAccountName { get; set; }
    public string DepGlAccountId { get; set; }
    public string DepGlAccountName { get; set; }
    public string ProfitGlAccountId { get; set; }
    public string ProfitGlAccountName { get; set; }
    public string LossGlAccountId { get; set; }
    public string LossGlAccountName { get; set; }
}