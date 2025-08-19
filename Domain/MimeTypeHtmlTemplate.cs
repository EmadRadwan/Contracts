namespace Domain;

public class MimeTypeHtmlTemplate
{
    public string MimeTypeId { get; set; } = null!;
    public string? TemplateLocation { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public MimeType MimeType { get; set; } = null!;
}