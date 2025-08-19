namespace Application.WorkEfforts;

public class ComponentLocation
{
    public string ProductId { get; set; }
    public string LocationSeqId { get; set; }
    public string SecondaryLocationSeqId { get; set; }
    public string FailIfItemsAreNotAvailable { get; set; } = "Y";
}

