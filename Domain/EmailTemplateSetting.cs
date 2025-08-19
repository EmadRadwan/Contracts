namespace Domain;

public class EmailTemplateSetting
{
    public string EmailTemplateSettingId { get; set; } = null!;
    public string? EmailType { get; set; }
    public string? Description { get; set; }
    public string? BodyScreenLocation { get; set; }
    public string? XslfoAttachScreenLocation { get; set; }
    public string? FromAddress { get; set; }
    public string? CcAddress { get; set; }
    public string? BccAddress { get; set; }
    public string? Subject { get; set; }
    public string? ContentType { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Enumeration? EmailTypeNavigation { get; set; }
}