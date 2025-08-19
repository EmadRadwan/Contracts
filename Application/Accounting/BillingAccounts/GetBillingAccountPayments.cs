using Application.Order.Orders;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.BillingAccounts;

public class GetBillingAccountPayments
{
    public class Query : IRequest<Result<List<BillingAccountPaymentDto>>>
    {
        public string BillingAccountId { get; set; }

        public Query(string billingAccountId)
        {
            BillingAccountId = billingAccountId;
        }
    }

    public class
        GetBillingAccountPaymentsAndListHandler : IRequestHandler<Query, Result<List<BillingAccountPaymentDto>>>
    {
        private readonly DataContext _context;

        public GetBillingAccountPaymentsAndListHandler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<BillingAccountPaymentDto>>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            try
            {
                var billingAccountId = request.BillingAccountId;

                // Step 1: Fetch BillingAccount and associated BillingAccountRoles using underlying tables
                var billingAccount = await _context.BillingAccounts
                    .Include(ba => ba.BillingAccountRoles) // Directly include BillingAccountRoles
                    .FirstOrDefaultAsync(ba => ba.BillingAccountId == billingAccountId, cancellationToken);

                if (billingAccount == null)
                    return Result<List<BillingAccountPaymentDto>>.Failure("BillingAccount not found.");

                // Step 2: Identify the BILL_TO_CUSTOMER role from BillingAccountRoles
                var billToCustomerRole = billingAccount.BillingAccountRoles
                    .FirstOrDefault(bar => bar.RoleTypeId == "BILL_TO_CUSTOMER");

                // Optional: If you need to fetch related Party or other entities based on BillingAccountRole
                // Example: Assuming BillingAccountRole has a navigation property to Party
                // var billToCustomer = billToCustomerRole?.Party;

                // Step 3: Retrieve payments associated with the billing account

                var payments = await _context.Payments
                    .Join(_context.PaymentApplications,
                        p => p.PaymentId,
                        pa => pa.PaymentId,
                        (p, pa) => new { p, pa })
                    .Where(p => p.pa.BillingAccountId == billingAccountId)
                    .ToListAsync(cancellationToken);

                // Step 4: Map payments to DTOs
                var result = payments.Select(p => new BillingAccountPaymentDto
                {
                    BillingAccountId = p.pa.BillingAccountId,
                    PaymentId = p.p.PaymentId,
                    PaymentMethodTypeId = p.p.PaymentMethodTypeId,
                    InvoiceId = p.pa.InvoiceId,
                    InvoiceItemSeqId = p.pa.InvoiceItemSeqId,
                    EffectiveDate = p.p.EffectiveDate ?? default,
                    AmountApplied = p.pa.AmountApplied ?? 0m,
                    Amount = p.p.Amount
                }).ToList();


                return Result<List<BillingAccountPaymentDto>>.Success(result);
            }
            catch (System.Exception ex)
            {
                // Log the exception as needed (not shown here)
                return Result<List<BillingAccountPaymentDto>>.Failure(
                    $"Error retrieving billing account payments: {ex.Message}");
            }
        }
    }
}