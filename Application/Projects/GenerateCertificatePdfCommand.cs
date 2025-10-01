namespace Application.Projects;

public class GenerateCertificatePdfCommand
{
    public string CertificateType { get; set; }
    public Certificate Certificate { get; set; }
    public List<CertificateItem> Items { get; set; }
    public double Subtotal { get; set; }
    public Dictionary<string, string> Translations { get; set; }
}