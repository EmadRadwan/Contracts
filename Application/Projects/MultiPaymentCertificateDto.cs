using System;
using System.Collections.Generic;

namespace Application.Projects
{
    public class MultiPaymentCertificateDto
    {
        public string WorkEffortId { get; set; }
        public string Code { get; set; }
        public DateTime? Date { get; set; }
        public string Description { get; set; }
        public string PaymentMethodId { get; set; }
        public string? ChequeNumber { get; set; }
        public DateTime? ChequeDate { get; set; }
        public string? CurrentStatusId { get; set; }
        public string? StatusDescription { get; set; }
        public string? StatusDescriptionArabic { get; set; }
        public string? GlAccountIdAdvancedPayment { get; set; }
        public List<MultiPaymentItemDto> Items { get; set; }
    }

   
}