using Application.Shipments;
using Domain;

namespace Application.Facilities;

public class LoadPackingDataResult
{
    public string FacilityId { get; set; }
    public string OrderId { get; set; }
    public string ShipGroupSeqId { get; set; }
    public List<PackingItemDto> Items { get; set; } = new List<PackingItemDto>();
    public List<string> InvoiceIds { get; set; } = new List<string>();
}