namespace Application.Projects;

public interface IPdfGenerationService
{
    Task<byte[]> GenerateCertificatePdfAsync(
        string certificateType,
        Certificate certificate,
        List<CertificateItem> items,
        double subtotal,
        Dictionary<string, string> translations);
}