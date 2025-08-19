namespace Application.Shipments.Payments
{
    public class AuthResult
    {
        public string AuthCode { get; set; }
        public bool AuthResultFlag { get; set; }
        public decimal ProcessAmount { get; set; }
        public string AuthRefNum { get; set; }
        public string AuthFlag { get; set; }
        public string AuthMessage { get; set; }
        public string CvCode { get; set; }
        public string AvsCode { get; set; }
        public string ScoreCode { get; set; }
        public string CaptureRefNum { get; set; }
        public bool? CaptureResult { get; set; } // Nullable to account for cases where it might not be set
        public string CaptureCode { get; set; }
        public string CaptureFlag { get; set; }
        public string CaptureMessage { get; set; }

        // New properties to handle errors
        public bool IsError { get; set; }
        public string ErrorMessage { get; set; }

        // Optionally, you can override ToString() for debugging/logging purposes
        public override string ToString()
        {
            return $"AuthCode: {AuthCode}, AuthResultFlag: {AuthResultFlag}, ProcessAmount: {ProcessAmount}, " +
                   $"AuthRefNum: {AuthRefNum}, AuthFlag: {AuthFlag}, AuthMessage: {AuthMessage}, CvCode: {CvCode}, " +
                   $"AvsCode: {AvsCode}, ScoreCode: {ScoreCode}, CaptureRefNum: {CaptureRefNum}, CaptureResult: {CaptureResult}, " +
                   $"CaptureCode: {CaptureCode}, CaptureFlag: {CaptureFlag}, CaptureMessage: {CaptureMessage}, " +
                   $"IsError: {IsError}, ErrorMessage: {ErrorMessage}";
        }
    }
}
