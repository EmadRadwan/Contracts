namespace Domain;

public class VisualThemeSet
{
    public VisualThemeSet()
    {
        VisualThemes = new HashSet<VisualTheme>();
        WebSites = new HashSet<WebSite>();
    }

    public string VisualThemeSetId { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<VisualTheme> VisualThemes { get; set; }
    public ICollection<WebSite> WebSites { get; set; }
}