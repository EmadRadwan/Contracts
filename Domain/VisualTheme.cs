namespace Domain;

public class VisualTheme
{
    public VisualTheme()
    {
        VisualThemeResources = new HashSet<VisualThemeResource>();
    }

    public string VisualThemeId { get; set; } = null!;
    public string? VisualThemeSetId { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public VisualThemeSet? VisualThemeSet { get; set; }
    public ICollection<VisualThemeResource> VisualThemeResources { get; set; }
}