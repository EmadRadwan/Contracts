using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Accounting.Payments;
using Application.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Projects
{
    public class GetProjectReport
    {
        public class Query : IRequest<ProjectReportDto>
        {
            public string ProjectId { get; set; } = null!;
            public DateTime? ExpensesStartDate { get; set; }
            public DateTime? ExpensesEndDate { get; set; }
            public bool ExpensesAllData { get; set; } = true;
            public DateTime? RevenuesStartDate { get; set; }
            public DateTime? RevenuesEndDate { get; set; }
            public bool RevenuesAllData { get; set; } = true;
        }

        public class Handler : IRequestHandler<Query, ProjectReportDto>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<ProjectReportDto> Handle(Query request, CancellationToken cancellationToken)
            {
                var dto = new ProjectReportDto
                {
                    Expenses = await GetExpenses(request, cancellationToken),
                    Revenues = await GetRevenues(request, cancellationToken),
                    DirectPayments = await GetDirectPayments(request, cancellationToken),
                    OperatingExpenses = await GetOperatingExpenses(request, cancellationToken),
                    AccountingTransactions = await GetAccountingTransactions(request, cancellationToken)
                };

                // Apply deduplication
                DeduplicateExpenses(dto);

                // Generate summaries (This is the main new value)
                dto.CostSummary = GenerateCostSummary(dto);
                dto.FinancialSummary = GenerateFinancialSummary(dto);

                return dto;
            }

            private void DeduplicateExpenses(ProjectReportDto dto)
            {
                var expensePaymentIds = dto.Expenses
                    .Where(e => !string.IsNullOrEmpty(e.PaymentId))
                    .Select(e => e.PaymentId!)
                    .ToHashSet();

                dto.DirectPayments = dto.DirectPayments
                    .Where(dp => !expensePaymentIds.Contains(dp.PaymentId))
                    .ToList();

                var allUsedIds = new HashSet<string>(expensePaymentIds);
                foreach (var dp in dto.DirectPayments) allUsedIds.Add(dp.PaymentId);

                dto.OperatingExpenses = dto.OperatingExpenses
                    .Where(oe => !allUsedIds.Contains(oe.PaymentId))
                    .ToList();
            }

            // ==================== NEW SUMMARY GENERATORS ====================

            private ProjectCostSummaryDto GenerateCostSummary(ProjectReportDto dto)
            {
                var summary = new ProjectCostSummaryDto();

                // Direct Expenses
                summary.TotalDirectExpenses = dto.Expenses.Sum(e => e.NetCertifiedAmount) 
                                           + dto.DirectPayments.Sum(p => p.Amount);

                // Operating Expenses
                summary.TotalOperatingExpenses = dto.OperatingExpenses.Sum(p => p.Amount);

                // Accounting
                summary.TotalAccountingTransactions = dto.AccountingTransactions.Sum(p => p.Amount);

                summary.GrandTotalExpenses = summary.TotalDirectExpenses 
                                           + summary.TotalOperatingExpenses 
                                           + summary.TotalAccountingTransactions;

                // Category Breakdowns
                summary.MainCategories = dto.OperatingExpenses
                    .GroupBy(p => p.PaymentTypeDescription ?? "أخرى")
                    .ToDictionary(g => g.Key, g => g.Sum(p => p.Amount));

                // Marketing Breakdown
                summary.MarketingBreakdown = dto.OperatingExpenses
                    .Where(p => p.PaymentTypeDescription?.Contains("دعاية") == true 
                             || p.PaymentTypeDescription?.Contains("إعلان") == true
                             || p.Comments?.Contains("دعاية") == true)
                    .GroupBy(p => p.PartyIdFromName ?? "غير مصنف")
                    .ToDictionary(g => g.Key, g => g.Sum(p => p.Amount));

                // You can extend ConstructionBreakdown & SubcontractorBreakdown similarly from Expenses list

                return summary;
            }

            private ProjectFinancialSummaryDto GenerateFinancialSummary(ProjectReportDto dto)
            {
                return new ProjectFinancialSummaryDto
                {
                    TotalCollectedRevenue = dto.Revenues.Sum(r => r.CollectedAmount),
                    TotalScheduledRevenue = dto.Revenues.Sum(r => r.ScheduledAmount),
                    OutstandingRevenue = dto.Revenues.Sum(r => r.OutstandingAmount),
                    OverdueRevenue = dto.Revenues.Sum(r => r.LateAmount),
                    FutureRevenue = dto.Revenues.Sum(r => r.FutureAmount)
                };
            }

            // Keep your existing private methods (GetExpenses, GetRevenues, etc.) unchanged
            // ... (paste your original GetExpenses, GetRevenues, GetDirectPayments, etc. here)
        }
    }
}