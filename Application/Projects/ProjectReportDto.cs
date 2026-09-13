using System;
using System.Collections.Generic;
using System.Linq;
using Application.Accounting.Payments;
using Application.Order.SalesRequests;

namespace Application.Projects
{
    public class ProjectReportDto
    {
        public List<ProjectExpenseRecord> Expenses { get; set; } = new();
        public List<ProjectRevenueRecord> Revenues { get; set; } = new();
        public List<PaymentRecord> DirectPayments { get; set; } = new();
        public List<PaymentRecord> OperatingExpenses { get; set; } = new();
        public List<PaymentRecord> AccountingTransactions { get; set; } = new();
        public List<PaymentRecord> Payroll { get; set; } = new();
        public List<SalesRequestOrApartmentRecord> ApartmentSales { get; set; } = new();
        public List<ProjectCommissionPaymentRecord> PaidCommissions { get; set; } = new();

        /// <summary>
        /// Server-computed roll-ups for the report's summary. Authoritative — the in-app screen and
        /// the Excel/PDF exports all read these instead of re-summing the collections in the browser,
        /// so the three surfaces cannot drift. Ported verbatim from the old client-side summary block
        /// in client-app/src/features/Projects/report/ProjectReportExcel.tsx.
        /// </summary>
        public ProjectReportSummaryDto Summary { get; set; } = new();
    }

    public class ProjectReportSummaryDto
    {
        // ===== المصاريف — expenses, computed from the de-duplicated collections the DTO carries =====
        public decimal CertificateExpenses { get; set; }       // Σ Expenses.NetCertifiedAmount
        public decimal DirectPayments { get; set; }             // Σ DirectPayments.Amount (after dedup)
        public decimal AccountingTransactions { get; set; }     // Σ AccountingTransactions.Amount
        public decimal ProjectPayroll { get; set; }             // Σ Payroll.Amount (GL-entry rows → mirrors the project trial balance)
        public decimal OperatingExpenses { get; set; }          // Σ OperatingExpenses.Amount (after dedup)
        public decimal TotalProjectExpenses { get; set; }       // sum of the five above

        // ===== الإيرادات / وديعة الصيانة — maintenance deposit is custodial, split out =====
        public decimal RevenueScheduled { get; set; }
        public decimal RevenueCollected { get; set; }
        public decimal RevenueOutstanding { get; set; }
        public decimal MaintenanceScheduled { get; set; }
        public decimal MaintenanceCollected { get; set; }
        public decimal MaintenanceOutstanding { get; set; }

        // ===== مبيعات الوحدات =====
        public int UnitsSold { get; set; }
        public decimal UnitsSoldValue { get; set; }
        public decimal UnitsAdvanceCollected { get; set; }
        public int UnitsAvailable { get; set; }

        // ===== العمولات — paid vs. still-owed =====
        public int CommissionPaymentCount { get; set; }
        public decimal CommissionsPaid { get; set; }
        public decimal CommissionsPending { get; set; }

        // ===== مبلغ الإدارة — % of agreed revenue for the non-excluded buildings, less operating expenses =====
        public decimal MgmtFeeBase { get; set; }
        public decimal MgmtFeePercent { get; set; }
        public decimal MgmtFee { get; set; }
        public decimal MgmtFeeNet { get; set; }                 // MgmtFee − OperatingExpenses
        public List<string> MgmtExcludedBuildings { get; set; } = new();

        // ===== الصافي =====
        public decimal NetAfterExpenses { get; set; }           // RevenueCollected − TotalProjectExpenses
        public decimal NetAfterPaidCommissions { get; set; }    // − CommissionsPaid

        /// <summary>
        /// Straight port of the old client-side summary block in
        /// client-app/src/features/Projects/report/ProjectReportExcel.tsx (the <c>expTotal</c> …
        /// <c>mgmtNet</c> section) — same inputs, same arithmetic, same order — so the numbers match
        /// the spreadsheet the client already knows. Call this with the collections the report DTO
        /// actually returns (i.e. after the direct-payment / operating-expense de-duplication).
        /// Shared by GetProjectReport and GetCompanyReport.
        /// </summary>
        public static ProjectReportSummaryDto Build(
            IEnumerable<ProjectExpenseRecord> expenses,
            IEnumerable<ProjectRevenueRecord> revenues,
            IEnumerable<PaymentRecord> directPayments,
            IEnumerable<PaymentRecord> operatingExpenses,
            IEnumerable<PaymentRecord> accountingTransactions,
            IEnumerable<PaymentRecord> payroll,
            IEnumerable<SalesRequestOrApartmentRecord> apartmentSales,
            IEnumerable<ProjectCommissionPaymentRecord> paidCommissions,
            decimal mgmtFeePercent,
            IEnumerable<string> excludedBuildings)
        {
            // ----- المصاريف -----
            // NetCertifiedAmount is always populated by the handler (gross − discount − deductions
            // − insurance + transportation + gratuities); the TSX '??' fallback never triggered.
            var certificateExpenses = expenses.Sum(e => e.NetCertifiedAmount);
            var directTotal = directPayments.Sum(p => p.Amount);
            var transTotal = accountingTransactions.Sum(p => p.Amount);
            // GL-entry rows straight from the project account — summing them mirrors the project
            // trial balance for payroll (see the payroll section builder), which is the figure to match.
            var payrollTotal = payroll.Sum(p => p.Amount);
            var opTotal = operatingExpenses.Sum(p => p.Amount);
            var totalExpenses = certificateExpenses + directTotal + transTotal + payrollTotal + opTotal;

            // ----- الإيرادات / وديعة الصيانة (custodial — kept out of agreed revenue & the fee base) -----
            static bool IsMaintenance(ProjectRevenueRecord r) =>
                r.PaymentTypeId == "RECEIPT_MAINTENANCE_AMOUNT" || r.RevenueCategory == "Maintenance Deposit";

            var revenueList = revenues as IReadOnlyList<ProjectRevenueRecord> ?? revenues.ToList();
            var agreedRevenues = revenueList.Where(r => !IsMaintenance(r)).ToList();
            var maintenanceRevenues = revenueList.Where(IsMaintenance).ToList();

            // ----- مبيعات الوحدات -----
            var apartmentList = apartmentSales as IReadOnlyList<SalesRequestOrApartmentRecord> ?? apartmentSales.ToList();
            var soldRows = apartmentList.Where(s => s.IsSold).ToList();
            var availableCount = apartmentList.Count(s => !s.IsSold);

            // ----- العمولات -----
            var commissionList = paidCommissions as IReadOnlyList<ProjectCommissionPaymentRecord> ?? paidCommissions.ToList();
            var commissionsPaid = commissionList.Where(c => c.IsPaid).Sum(c => c.Amount);
            var commissionsPending = commissionList.Where(c => !c.IsPaid).Sum(c => c.Amount);

            // ----- مبلغ الإدارة -----
            var excluded = excludedBuildings
                .Select(b => (b ?? string.Empty).Trim())
                .Where(b => b.Length > 0)
                .Distinct()
                .ToList();
            var excludedSet = excluded.ToHashSet();
            var mgmtBase = agreedRevenues
                .Where(r => !excludedSet.Contains((r.BuildingNumber ?? string.Empty).Trim()))
                .Sum(r => r.ScheduledAmount);
            var mgmtFee = mgmtBase * (mgmtFeePercent / 100m);

            var revenueCollected = agreedRevenues.Sum(r => r.CollectedAmount);

            return new ProjectReportSummaryDto
            {
                CertificateExpenses = certificateExpenses,
                DirectPayments = directTotal,
                AccountingTransactions = transTotal,
                ProjectPayroll = payrollTotal,
                OperatingExpenses = opTotal,
                TotalProjectExpenses = totalExpenses,

                RevenueScheduled = agreedRevenues.Sum(r => r.ScheduledAmount),
                RevenueCollected = revenueCollected,
                RevenueOutstanding = agreedRevenues.Sum(r => r.OutstandingAmount),
                MaintenanceScheduled = maintenanceRevenues.Sum(r => r.ScheduledAmount),
                MaintenanceCollected = maintenanceRevenues.Sum(r => r.CollectedAmount),
                MaintenanceOutstanding = maintenanceRevenues.Sum(r => r.OutstandingAmount),

                UnitsSold = soldRows.Count,
                UnitsSoldValue = soldRows.Sum(s => s.TotalPrice ?? 0m),
                UnitsAdvanceCollected = soldRows.Sum(s => s.AdvancePayment ?? 0m),
                UnitsAvailable = availableCount,

                CommissionPaymentCount = commissionList.Count,
                CommissionsPaid = commissionsPaid,
                CommissionsPending = commissionsPending,

                MgmtFeeBase = mgmtBase,
                MgmtFeePercent = mgmtFeePercent,
                MgmtFee = mgmtFee,
                MgmtFeeNet = mgmtFee - opTotal,
                MgmtExcludedBuildings = excluded,

                NetAfterExpenses = revenueCollected - totalExpenses,
                NetAfterPaidCommissions = revenueCollected - totalExpenses - commissionsPaid
            };
        }
    }

    // One row per commission payee-payment (a commission fans out to one payment per payee at
    // approval — sales rep, manager, broker company, etc.). Sourced from Payment, not the GL:
    // COMMISSION_PAYMENT rows are booked outside the project's GL account tree, so they never
    // appear in the expense sections and cannot double-count against them.
    public class ProjectCommissionPaymentRecord
    {
        public string PaymentId { get; set; } = null!;
        public string? SalesCommissionId { get; set; }
        public string? SalesRequestId { get; set; }
        public string? SaleTypeId { get; set; }
        public string? CommissionStatusId { get; set; }
        public string? CommissionStatusArabic { get; set; }
        public string? ApartmentId { get; set; }
        public string? ApartmentName { get; set; }
        public string? BuildingNumber { get; set; }
        public string? PayeePartyId { get; set; }
        public string? PayeeName { get; set; }
        public decimal Amount { get; set; }
        // PMNT_SENT / PMNT_CONFIRMED → actually disbursed; anything else is still owed.
        public bool IsPaid { get; set; }
        public string? PaymentStatusId { get; set; }
        public string? PaymentStatusArabic { get; set; }
        public string? PaymentMethodTypeArabic { get; set; }
        public DateOnly? EffectiveDate { get; set; }
        public DateTime? CreatedStamp { get; set; }
        public string? ChequeNumber { get; set; }
        public DateOnly? ChequeDate { get; set; }
        public string? Comments { get; set; }
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
        // Due-status buckets, each its own field so the report can render one filterable column per
        // value. Only one is set per (uncollected) row; collected rows leave all four null.
        public string? DeservedToday { get; set; }
        public string? DeservedWithinWeek { get; set; }
        public string? DeservedWithinMonth { get; set; }
        public string? LateDue { get; set; }
        public int? Year { get; set; }
        public string? Quarter { get; set; }
    }
}
