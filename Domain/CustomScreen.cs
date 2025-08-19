namespace Domain;

public class CustomScreen
{
    public string CustomScreenId { get; set; } = null!;
    public string? CustomScreenTypeId { get; set; }
    public string? CustomScreenName { get; set; }
    public string? CustomScreenLocation { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public CustomScreenType? CustomScreenType { get; set; }
}