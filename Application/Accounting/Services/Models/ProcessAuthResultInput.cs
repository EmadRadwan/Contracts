using System.Globalization;
using Domain;

namespace Application.Accounting.Services.Models;

public class ProcessAuthResultInput
{
    // Order payment preference
    public OrderPaymentPreference OrderPaymentPreference { get; set; }
    // Authorization result
    public bool? AuthResult { get; set; }
    // Authorization type
    public string ServiceTypeEnum { get; set; }
    // Currency code
    public string CurrencyUomId { get; set; }
    // User login
    public UserLogin UserLogin { get; set; }
    // Locale
    public CultureInfo Locale { get; set; }
    // AVS code
    public string AvsCode { get; set; }
    // CVV code
    public string CvCode { get; set; }
    // Fraud score code
    public string ScoreCode { get; set; }
    // Processed amount
    public decimal ProcessAmount { get; set; }
    // Authorization reference number
    public string AuthRefNum { get; set; }
    // Alternate reference number
    public string AuthAltRefNum { get; set; }
    // Authorization code
    public string AuthCode { get; set; }
    // Authorization flag
    public string AuthFlag { get; set; }
    // Authorization message
    public string AuthMessage { get; set; }
    // Declined flag
    public bool? ResultDeclined { get; set; }
    // NSF flag
    public bool? ResultNsf { get; set; }
    // Bad expiration flag
    public bool? ResultBadExpire { get; set; }
    // Bad card number flag
    public bool? ResultBadCardNumber { get; set; }
    // Internal response messages
    public List<string> InternalRespMsgs { get; set; }
}
