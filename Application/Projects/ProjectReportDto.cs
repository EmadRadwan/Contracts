using System;
using System.Collections.Generic;
using Application.Accounting.Payments;

namespace Application.Projects
{
    public class ProjectReportDto
    {
        public List<ProjectExpenseRecord> Expenses { get; set; } = new();
        public List<ProjectRevenueRecord> Revenues { get; set; } = new();
        public List<PaymentRecord> DirectPayments { get; set; } = new();
        public List<PaymentRecord> OperatingExpenses { get; set; } = new();
    }

    public class ProjectExpenseRecord
    {
        public string? ExpenseItemKey { get; set; }
        public string? CertificateKey { get; set; }
        public string? CertificateNumber { get; set; }
        public string? PaymentId { get; set; }
        public string? ProjectId { get; set; }
        public string? PartyId { get; set; }
        public string? PartyName { get; set; }
        public string? PartyRole { get; set; }
        public string? ProductId { get; set; }
        public string? ProductName { get; set; }
        public DateTime? ExpenseDate { get; set; }
        public string? RecordType { get; set; }
        public string? CertificateType { get; set; }
        public string? CertificateCategoryCode { get; set; }
        public string? CertificateDescription { get; set; }
        public string? ItemDescription { get; set; }
        public string? RelatedPurchaseOrderId { get; set; }
        public bool IsSupplyProcurement { get; set; }
        public bool IsWorkmanship { get; set; }
        public bool IsMultiPaymentCertificate { get; set; }
        public decimal Quantity { get; set; }
        public decimal? UnitRate { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal DeductionsAmount { get; set; }
        public decimal InsuranceAmount { get; set; }
        public decimal TransportationExpensesAmount { get; set; }
        public decimal GratuitiesAmount { get; set; }
        public decimal NetCertifiedAmount { get; set; }
        public decimal AchievementPercentage { get; set; }
        public string? CertificateTypeArabic { get; set; }
    }

    public class ProjectRevenueRecord
    {
        public string? PaymentId { get; set; }
        public string? StatusId { get; set; }
        public string? StatusDescription { get; set; }
        public string? SalesRequestId { get; set; }
        public string? ApartmentId { get; set; }
        public string? BuildingNumber { get; set; }
        public string? ProjectId { get; set; }
        public string? ProjectName { get; set; }
        public string? CustomerPartyId { get; set; }
        public string? CustomerName { get; set; }
        public string? PaymentTypeId { get; set; }
        public string? PaymentTypeArabic { get; set; }
        public string? RevenueCategory { get; set; }
        public decimal ScheduledAmount { get; set; }
        public decimal CollectedAmount { get; set; }
        public decimal OutstandingAmount { get; set; }
        public decimal LateAmount { get; set; }
        public decimal FutureAmount { get; set; }
        public string? PaymentStatus { get; set; }
        public string? OverdueBucket { get; set; }
        public int DaysOverdue { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? Comments { get; set; }
        public string? ChequeNumber { get; set; }
        public string? DueStatusArabic { get; set; }
        public int? Year { get; set; }
        public string? Quarter { get; set; }
    }
}
