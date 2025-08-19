using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.BillingAccounts;

public class GetBillingAccountOrders
{
    public class Query : IRequest<Result<List<OrderPaymentPrefDto>>>
    {
        public string BillingAccountId { get; set; }

        public Query(string billingAccountId)
        {
            BillingAccountId = billingAccountId;
        }
    }

    public class GetBillingAccountOrdersHandler : IRequestHandler<Query, Result<List<OrderPaymentPrefDto>>>
    {
        private readonly DataContext _context;

        public GetBillingAccountOrdersHandler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<OrderPaymentPrefDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            try
            {
                var billingAccountId = request.BillingAccountId;

                // Step 1: Fetch BillingAccount and associated BillingAccountRoles
                var billingAccount = await _context.BillingAccounts
                    .Include(ba => ba.BillingAccountRoles)
                    .FirstOrDefaultAsync(ba => ba.BillingAccountId == billingAccountId, cancellationToken);

                if (billingAccount == null)
                    return Result<List<OrderPaymentPrefDto>>.Failure("BillingAccount not found.");

                // Step 2: Identify the BILL_TO_CUSTOMER role from BillingAccountRoles
                var billToCustomerRole = billingAccount.BillingAccountRoles
                    .FirstOrDefault(bar => bar.RoleTypeId == "BILL_TO_CUSTOMER");

                // Step 3: Explicitly join OrderHeader with OrderPaymentPreference to filter orders
                var ordersQuery = from oh in _context.OrderHeaders
                    join ohapp in _context.OrderPaymentPreferences
                        on oh.OrderId equals ohapp.OrderId
                    join sts in _context.StatusItems
                        on ohapp.StatusId equals sts.StatusId
                    where oh.BillingAccountId == billingAccountId
                          && ohapp.PaymentMethodTypeId == "EXT_BILLACT"
                          && ohapp.StatusId == "PAYMENT_NOT_RECEIVED"
                    select new { OrderHeader = oh, OrderPaymentPreference = ohapp, StatusItem = sts };

                // Step 5: Map the data to DTOs
                var result = await ordersQuery.Select(x => new OrderPaymentPrefDto
                {
                    BillingAccountId = x.OrderHeader.BillingAccountId,
                    OrderId = x.OrderHeader.OrderId,
                    OrderDate = (DateTime)x.OrderHeader.OrderDate,
                    PaymentStatusId = x.OrderPaymentPreference.StatusId,
                    PaymentStatusDescription = x.StatusItem.Description,
                    MaxAmount = x.OrderPaymentPreference.MaxAmount,
                    // Map other necessary fields as required
                }).ToListAsync(cancellationToken: cancellationToken);


                return Result<List<OrderPaymentPrefDto>>.Success(result);
            }
            catch (System.Exception ex)
            {
                // Ideally, log the exception here using a logging framework like Serilog or NLog
                return Result<List<OrderPaymentPrefDto>>.Failure(
                    $"Error retrieving billing account orders: {ex.Message}");
            }
        }
    }
}