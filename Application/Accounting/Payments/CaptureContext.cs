using Domain;

namespace Application.Accounting.Payments;


public class CaptureContext
{
    public OrderPaymentPreference OrderPaymentPreference { get; set; }
    public string ServiceTypeEnum { get; set; }
    public string PayToPartyId { get; set; }
    public bool CaptureResult { get; set; }
    public decimal CaptureAmount { get; set; }
    public string CaptureRefNum { get; set; }
    public UserLogin UserLogin { get; set; }
    public string CurrencyUomId { get; set; }
}