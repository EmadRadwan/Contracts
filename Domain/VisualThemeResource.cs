namespace Domain;

public class VisualThemeResource
{
    public string VisualThemeId { get; set; } = null!;
    public string ResourceTypeEnumId { get; set; } = null!;
    public string SequenceId { get; set; } = null!;
    public string? ResourceValue { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Enumeration ResourceTypeEnum { get; set; } = null!;
    public VisualTheme VisualTheme { get; set; } = null!;
}