namespace Domain;

public class WebSitePublishPoint
{
    public string ContentId { get; set; } = null!;
    public string? TemplateTitle { get; set; }
    public string? StyleSheetFile { get; set; }
    public string? Logo { get; set; }
    public string? MedallionLogo { get; set; }
    public string? LineLogo { get; set; }
    public string? LeftBarId { get; set; }
    public string? RightBarId { get; set; }
    public string? ContentDept { get; set; }
    public string? AboutContentId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Content Content { get; set; } = null!;
}