namespace Domain;

public class ReturnHeader
{
    public ReturnHeader()
    {
        CommunicationEventReturns = new HashSet<CommunicationEventReturn>();
        ReturnAdjustments = new HashSet<ReturnAdjustment>();
        ReturnContactMeches = new HashSet<ReturnContactMech>();
        ReturnItemBillings = new HashSet<ReturnItemBilling>();
        ReturnItemShipments = new HashSet<ReturnItemShipment>();
        ReturnItems = new HashSet<ReturnItem>();
        ReturnStatuses = new HashSet<ReturnStatus>();
        Shipments = new HashSet<Shipment>();
        TrackingCodeOrderReturns = new HashSet<TrackingCodeOrderReturn>();
    }

    public string ReturnId { get; set; } = null!;
    public string? ReturnHeaderTypeId { get; set; }
    public string? StatusId { get; set; }
    public string? CreatedBy { get; set; }
    public string? FromPartyId { get; set; }
    public string? ToPartyId { get; set; }
    public string? PaymentMethodId { get; set; }
    public string? FinAccountId { get; set; }
    public string? BillingAccountId { get; set; }
    public DateTime? EntryDate { get; set; }
    public string? OriginContactMechId { get; set; }
    public string? DestinationFacilityId { get; set; }
    public string? NeedsInventoryReceive { get; set; }
    public string? CurrencyUomId { get; set; }
    public string? SupplierRmaId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public BillingAccount? BillingAccount { get; set; }
    public Uom? CurrencyUom { get; set; }
    public Facility? DestinationFacility { get; set; }
    public FinAccount? FinAccount { get; set; }
    public Party? FromParty { get; set; }
    public ContactMech? OriginContactMech { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public ReturnHeaderType? ReturnHeaderType { get; set; }
    public StatusItem? Status { get; set; }
    public Party? ToParty { get; set; }
    public ICollection<CommunicationEventReturn> CommunicationEventReturns { get; set; }
    public ICollection<ReturnAdjustment> ReturnAdjustments { get; set; }
    public ICollection<ReturnContactMech> ReturnContactMeches { get; set; }
    public ICollection<ReturnItemBilling> ReturnItemBillings { get; set; }
    public ICollection<ReturnItemShipment> ReturnItemShipments { get; set; }
    public ICollection<ReturnItem> ReturnItems { get; set; }
    public ICollection<ReturnStatus> ReturnStatuses { get; set; }
    public ICollection<Shipment> Shipments { get; set; }
    public ICollection<TrackingCodeOrderReturn> TrackingCodeOrderReturns { get; set; }
}