using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Order.SalesRequests;

public class CalculateMeterPrice
{
    public class Query : IRequest<decimal>
    {
        public decimal CashPricePerM2 { get; set; }
        public decimal AnnualDiscountRate { get; set; }
        public decimal DownPaymentPercentage { get; set; }
        public int DurationYears { get; set; }
        public int InstallmentsPerYear { get; set; }
    }

    public class Handler : IRequestHandler<Query, decimal>
    {
        public Task<decimal> Handle(Query request, CancellationToken ct)
        {
            int n = request.DurationYears * request.InstallmentsPerYear;
            decimal r = request.AnnualDiscountRate / request.InstallmentsPerYear;

            // PVAF
            decimal pvaf = (1 - (decimal)Math.Pow(1 + (double)r, -n)) / r;

            // We solve:
            // Cash = DP% * P + ((1 - DP%) * P / n) * PVAF
            // Let P be the unknown (price)

            decimal DP = request.DownPaymentPercentage;
            decimal cash = request.CashPricePerM2;

            // Newton-Raphson Method
            decimal P = cash;           // initial guess
            for (int i = 0; i < 50; i++)
            {
                decimal A = (1 - DP) * P / n;                 // installment per period
                decimal f = DP * P + A * pvaf - cash;         // equation
                decimal df = DP + (1 - DP) * pvaf / n;        // derivative

                P -= f / df;                                  // update guess
            }

            return Task.FromResult(Math.Round(P, 0));
        }
    }
}
