using System;
using System.Collections.Generic;

namespace Application.Projects
{
    public class MultiPaymentCertificateDto
    {
        public string WorkEffortId { get; set; }
        public DateTime? Date { get; set; }
        public string Description { get; set; }
        public string? CurrentStatusId { get; set; }
        public string? CompanyId { get; set; }
        public string? StatusDescription { get; set; }
        public string? StatusDescriptionArabic { get; set; }
        public string? PartyIdEmployee { get; set; }
        public string? PartyEmployeeName { get; set; }
        public List<MultiPaymentItemDto> Items { get; set; }
    }

   
}