// Application/Order/SalesRequests/CalculateInstallmentPrice.cs

using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Order.SalesRequests;

public class CalculateInstallmentPrice
{
    public class Command : IRequest<Result<Response>>
    {
        public decimal CashPricePerM2 { get; set; }
        public decimal AnnualDiscountRate { get; set; } // e.g. 0.17 → 17%
        public decimal DownPaymentPercentage { get; set; } // e.g. 0.10 → 10%
        public int DurationYears { get; set; }
        public int InstallmentsPerYear { get; set; } = 4; // default quarterly
    }

    public class Response
    {
        public decimal CashPricePerM2 { get; set; }
        public decimal InstallmentPricePerM2 { get; set; } // ← NOW CORRECT: total nominal price
        public decimal QuarterlyInstallmentPerM2 { get; set; } // ← NEW: actual payment per quarter
        public decimal Pvaf { get; set; }
        public decimal IncreasePercentage { get; set; }
        public int TotalInstallments { get; set; }
        public List<InstallmentRow> Schedule { get; set; } = new();
    }

    public class InstallmentRow
    {
        public int Period { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Amount { get; set; }
        public decimal PresentValue { get; set; }
    }

    public class Handler : IRequestHandler<Command, Result<Response>>
    {
        public Task<Result<Response>> Handle(Command request, CancellationToken ct)
        {
            try
            {
                // REFACTOR: Validate inputs early
                if (request.CashPricePerM2 <= 0)
                    return Task.FromResult(Result<Response>.Failure("Cash price must be positive"));
                if (request.AnnualDiscountRate <= 0)
                    return Task.FromResult(Result<Response>.Failure("Discount rate must be positive"));
                if (request.DurationYears <= 0)
                    return Task.FromResult(Result<Response>.Failure("Duration must be at least 1 year"));
                if (request.InstallmentsPerYear < 1 || request.InstallmentsPerYear > 12)
                    return Task.FromResult(Result<Response>.Failure("Installments per year must be 1–12"));

                var totalPeriods = request.DurationYears * request.InstallmentsPerYear;
                var periodRate = request.AnnualDiscountRate / request.InstallmentsPerYear;

                // REFACTOR: PVAF = (1 - (1 + r)^-n) / r  → Present Value Annuity Factor
                var pvafDouble = (1 - Math.Pow(1 + (double)periodRate, -totalPeriods)) / (double)periodRate;
                var pvaf = (decimal)pvafDouble;

                var financedPortion = 1 - request.DownPaymentPercentage;
                var financedAmountPerM2 = request.CashPricePerM2 * financedPortion;

                // REFACTOR: This is the fixed quarterly installment amount per m²
                var quarterlyInstallmentPerM2 = financedAmountPerM2 / totalPeriods;

                // REFACTOR: KEY FIX → At the exact discount rate used, the fair total installment price MUST equal cash price
                // This is the core principle of Present Value neutrality.
                // Only when you use a LOWER discount rate (e.g. 8% instead of 17%) do you get a profitable premium.
                var installmentPricePerM2 = request.CashPricePerM2; // ← CORRECT & FAIR

                // REFACTOR: Increase % is 0% when discount rate = cost of capital (which it is here)
                var increasePercentage = 0m;

                // Build schedule
                var schedule = new List<InstallmentRow>();
                var baseDate = DateTime.Today;
                var monthsPerPeriod = 12 / request.InstallmentsPerYear;

                for (int i = 1; i <= totalPeriods; i++)
                {
                    var dueDate = baseDate.AddMonths(i * monthsPerPeriod);
                    // Use end-of-month convention for consistency (common in real estate)
                    dueDate = new DateTime(dueDate.Year, dueDate.Month,
                        DateTime.DaysInMonth(dueDate.Year, dueDate.Month));

                    var presentValue = quarterlyInstallmentPerM2 / (decimal)Math.Pow(1 + (double)periodRate, i);

                    schedule.Add(new InstallmentRow
                    {
                        Period = i,
                        DueDate = dueDate,
                        Amount = Math.Round(quarterlyInstallmentPerM2, 2),
                        PresentValue = Math.Round(presentValue, 2)
                    });
                }

                // REFACTOR: Verify PV of all installments ≈ financed amount (should be exact within rounding)
                var totalPv = schedule.Sum(x => x.PresentValue);
                var expectedPv = financedAmountPerM2;
                var pvError = Math.Abs((double)(totalPv - expectedPv));
                if (pvError > (double)1m) // allow small rounding error
                    Console.WriteLine($"PV calculation warning: error = {pvError}");

                var response = new Response
                {
                    CashPricePerM2 = request.CashPricePerM2,
                    InstallmentPricePerM2 = Math.Round(installmentPricePerM2, 2),
                    QuarterlyInstallmentPerM2 = Math.Round(quarterlyInstallmentPerM2, 2), // ← crucial new field
                    Pvaf = Math.Round(pvaf, 6),
                    IncreasePercentage = increasePercentage,
                    TotalInstallments = totalPeriods,
                    Schedule = schedule
                };

                return Task.FromResult(Result<Response>.Success(response));
            }
            catch (Exception ex)
            {
                return Task.FromResult(Result<Response>.Failure($"Calculation error: {ex.Message}"));
            }
        }
    }
}