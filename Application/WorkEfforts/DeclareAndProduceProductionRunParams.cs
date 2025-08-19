namespace Application.WorkEfforts;

public class DeclareAndProduceProductionRunParams
{
    public string WorkEffortId { get; set; }
    public string FacilityId { get; set; }
    public string? ProductId { get; set; }
    public decimal? Quantity { get; set; }
    public string? InventoryItemTypeId { get; set; }
    public string? LocationSeqId { get; set; }
    public string? LotId { get; set; }
    public bool? CreateLotIfNeeded { get; set; }
    public bool? AutoCreateLot { get; set; }
    public List<ComponentLocation>? ComponentsLocationMap { get; set; }
}