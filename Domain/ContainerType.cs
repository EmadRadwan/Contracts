namespace Domain;

public class ContainerType
{
    public ContainerType()
    {
        Containers = new HashSet<Container>();
    }

    public string ContainerTypeId { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<Container> Containers { get; set; }
}