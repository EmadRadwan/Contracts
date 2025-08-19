namespace Domain;

public class X509IssuerProvision
{
    public string CertProvisionId { get; set; } = null!;
    public string? CommonName { get; set; }
    public string? OrganizationalUnit { get; set; }
    public string? OrganizationName { get; set; }
    public string? CityLocality { get; set; }
    public string? StateProvince { get; set; }
    public string? Country { get; set; }
    public string? SerialNumber { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }
}