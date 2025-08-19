namespace Application.Facilities;

public class PackBulkItemsInput
{
    // Mandatory
    public string OrderId { get; set; }           // The unique order we're packing
    public string ShipGroupSeqId { get; set; }    // The shipping group ID in the order

    // Optional
    public bool UpdateQuantity { get; set; } = false;  // If true, line items override existing quantities
    public string PickerPartyId { get; set; }          // The ID of the party picking the items
    public string HandlingInstructions { get; set; }   // e.g. "Handle with care"

    // If using nextPackageSeq you can store it here as well:
    public int? NextPackageSeq { get; set; }

    // Lines that represent items to pack (replaces dictionaries in Groovy)
    public List<PackBulkLine> Lines { get; set; } = new List<PackBulkLine>();
}
