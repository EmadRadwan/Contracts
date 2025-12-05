using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.BillingAccounts;

public class GetBalancesForVendorAndProject
{
    public record Query(string PartyId, string ProjectId) : IRequest<Result<BalancesDto>>;

    public record BalancesDto(
        decimal InitialBalance,
        decimal UsedBalance,
        decimal RemainingBalance,
        string? Message = null);

    public class Handler : IRequestHandler<Query, Result<BalancesDto>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<BalancesDto>> Handle(Query request, CancellationToken ct)
        {
            var (partyId, projectId) = (request.PartyId, request.ProjectId);

            // Step 1: Find active BillingAccount linked to this vendor (partyId) and project (WorkEffortId)
            var billingAccountQuery = from ba in _context.BillingAccounts
                                      join bar in _context.BillingAccountRoles on ba.BillingAccountId equals bar.BillingAccountId
                                      where bar.PartyId == partyId
                                         && bar.RoleTypeId == "BILL_FROM_VENDOR"
                                         && ba.WorkEffortId == projectId
                                         && ba.FromDate <= DateTime.UtcNow
                                         && (ba.ThruDate == null || ba.ThruDate > DateTime.UtcNow)
                                         && bar.FromDate <= DateTime.UtcNow
                                         && (bar.ThruDate == null || bar.ThruDate > DateTime.UtcNow)
                                      select ba;

            var billingAccount = await billingAccountQuery
                .FirstOrDefaultAsync(ct);

            if (billingAccount == null)
            {
                return Result<BalancesDto>.Success(new BalancesDto(
                    InitialBalance: 0m,
                    UsedBalance: 0m,
                    RemainingBalance: 0m,
                    Message: "لا يوجد حساب دفع مُعيَّن لهذا المورد على المشروع المحدد."
                ));
            }
            
            decimal initialBalance = billingAccount.AccountLimit ?? 0m;

            // Step 2: Sum all outgoing payments linked to this billing account + project
            // These are typically ADVANCE_TO_VENDOR_CONTRACTOR or similar, using EXT_BILLACT or direct billing account
            decimal usedBalance = await _context.Payments
                .Where(p =>
                    p.WorkEffortId == projectId &&
                    p.PartyIdTo == partyId &&
                    p.StatusId == "PMNT_SENT" &&
                    p.PaymentTypeId == "ADVANCE_TO_VENDOR_CONTRACTOR") // adjust as needed
                .SumAsync(p => (decimal?)p.Amount ?? 0m, ct);
            
            decimal remainingBalance = initialBalance - usedBalance;

            // Round to 2 decimal places consistently
            initialBalance = Math.Round(initialBalance, 2, MidpointRounding.AwayFromZero);
            usedBalance = Math.Round(usedBalance, 2, MidpointRounding.AwayFromZero);
            remainingBalance = Math.Round(remainingBalance, 2, MidpointRounding.AwayFromZero);

            var result = new BalancesDto(initialBalance, usedBalance, remainingBalance);

            return Result<BalancesDto>.Success(result);
        }
    }
}