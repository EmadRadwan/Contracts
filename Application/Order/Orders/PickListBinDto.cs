namespace Application.Order.Orders;

public class PickListBinDto
{
    public string PICKLIST_BIN_ID { get; set; }
    public string PICKLIST_ID { get; set; }
    public int BIN_LOCATION_NUMBER { get; set; }
    public string PRIMARY_ORDER_ID { get; set; }
    public string PRIMARY_SHIP_GROUP_SEQ_ID { get; set; }
    public DateTime LAST_UPDATED_STAMP { get; set; }
    public DateTime? LAST_UPDATED_TX_STAMP { get; set; }
    public DateTime CREATED_STAMP { get; set; }
    public DateTime? CREATED_TX_STAMP { get; set; }
}
