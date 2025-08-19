namespace Application.Accounting.Payments;


public class ProcessAuthResult
{
    // Authorization reply properties
    public string CcAuthReplyAuthorizationCode { get; set; }
    public string CcAuthReplyAmount { get; set; }
    public string RequestID { get; set; }
    public string CcAuthReplyReasonCode { get; set; }
    public string CcAuthReplyProcessorResponse { get; set; }
    public string CcAuthReplyCvCode { get; set; }
    public string CcAuthReplyAvsCode { get; set; }
    public string CcAuthReplyAuthFactorCode { get; set; }

    // Capture reply properties
    public string CcCaptureReplyReconciliationID { get; set; }
    public string CcCaptureReplyReasonCode { get; set; }

    // Decision
    public string Decision { get; set; }
}
