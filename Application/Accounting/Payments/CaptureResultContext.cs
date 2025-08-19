using Domain;
using System.Collections.Generic;

namespace Application.Shipments.Payments
{
    public class CaptureResultContext
    {
        public OrderPaymentPreference OrderPaymentPreference { get; set; }
        public UserLogin UserLogin { get; set; }
        public string ServiceTypeEnum { get; set; }
        public decimal? CaptureAmount { get; set; }
        public bool CaptureResult { get; set; }
        public decimal? ProcessAmount { get; set; }
        public string CaptureAltRefNum { get; set; }
        public string CaptureRefNum { get; set; }
        public string CaptureCode { get; set; }
        public string CaptureFlag { get; set; }
        public string CaptureMessage { get; set; }
        public List<string> InternalRespMsgs { get; set; } = new List<string>();
        public string PayToPartyId { get; set; } // Added property
        public string InvoiceId { get; set; } // Added property
        public string CurrencyUomId { get; set; } // Added property
    }
}