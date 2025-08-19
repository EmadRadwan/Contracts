namespace Domain;

public class CustomScreenType
{
    public CustomScreenType()
    {
        CustomScreens = new HashSet<CustomScreen>();
    }

    public string CustomScreenTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<CustomScreen> CustomScreens { get; set; }
}