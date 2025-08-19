namespace Domain;

public class FileExtension
{
    public string FileExtensionId { get; set; } = null!;
    public string? MimeTypeId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public MimeType? MimeType { get; set; }
}