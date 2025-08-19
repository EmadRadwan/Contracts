namespace Domain;

public class PortletCategory
{
    public PortletCategory()
    {
        PortletPortletCategories = new HashSet<PortletPortletCategory>();
    }

    public string PortletCategoryId { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<PortletPortletCategory> PortletPortletCategories { get; set; }
}