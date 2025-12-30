/*namespace Application.Projects;

public class GenerateCertificatePdfHandler
{
    private readonly IPdfGenerationService _pdfGenerationService;

    public GenerateCertificatePdfHandler(IPdfGenerationService pdfGenerationService)
    {
        _pdfGenerationService = pdfGenerationService;
    }

    public async Task<byte[]> HandleAsync(GenerateCertificatePdfCommand command)
    {
        // Validate input
        if (command.Certificate == null || command.Items == null)
        {
            throw new ArgumentException("Invalid certificate data");
        }

        return await _pdfGenerationService.GenerateCertificatePdfAsync(
            command.CertificateType,
            command.Certificate,
            command.Items,
            command.Subtotal,
            command.Translations);
    }
}*/