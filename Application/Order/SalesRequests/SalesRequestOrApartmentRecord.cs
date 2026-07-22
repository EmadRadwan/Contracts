namespace Application.Order.SalesRequests;

// Same shape as SalesRequestRecord, plus a flag distinguishing rows that come from an
// actual approved SalesRequest from rows synthesized from a Product that has no
// SalesRequest at all (i.e. still available/reserved inventory).
public class SalesRequestOrApartmentRecord : SalesRequestRecord
{
    public bool IsSold { get; set; }
}
