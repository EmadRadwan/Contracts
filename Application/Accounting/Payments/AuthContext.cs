using Domain;

namespace Application.Accounting.Payments;


public class AuthContext
{
    public OrderPaymentPreference OrderPaymentPreference { get; set; }
    public decimal ProcessAmount { get; set; }
    public string CurrencyUomId { get; set; }
    public bool AuthResult { get; set; }
    public string ServiceTypeEnum { get; set; }
    public string AuthCode { get; set; }
    public string AuthAltRefNum { get; set; }
    public string AuthRefNum { get; set; }
    public string AuthFlag { get; set; }
    public string AuthMessage { get; set; }
    public string CvCode { get; set; }
    public string AvsCode { get; set; }
    public string ScoreCode { get; set; }
    public string CaptureRefNum { get; set; }
    public List<string> InternalRespMsgs { get; set; }
}



