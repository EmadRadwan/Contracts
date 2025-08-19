namespace Domain;

public class ServiceSpecification
{
    public string ServiceSpecificationId { get; set; }
    public string ProductId { get; set; }
    public string MakeId { get; set; }
    public string ModelId { get; set; }
    public int StandardTimeInMinutes { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }


    // Navigation properties
    public virtual Product Product { get; set; }
    public virtual ProductCategory Make { get; set; }
    public virtual ProductCategory Model { get; set; }
}