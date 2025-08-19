using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Order.Orders;

public class ListApprovedSalesOrders
{
    public class Query : IRequest<Result<List<ApprovedSalesOrderDto>>>
    {
        public string? PartyId { get; set; }
    }

    // The MediatR handler that returns a list of approved sales order IDs.
    public class Handler : IRequestHandler<Query, Result<List<ApprovedSalesOrderDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<ApprovedSalesOrderDto>>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            try
            {
                // Query eligible sales orders.
                // Eligible orders are those with:
                //   - StatusId equal to "ORDER_APPROVED"
                //   - OrderTypeId equal to "SALES_ORDER"
                //   - Associated with the specified PartyId via OrderRoles (if provided)
                var eligibleOrdersQuery = _context.OrderHeaders.AsQueryable();

                eligibleOrdersQuery = eligibleOrdersQuery.Where(o =>
                    o.StatusId == "ORDER_APPROVED" &&
                    o.OrderTypeId == "SALES_ORDER");

                // REFACTOR: Join with OrderRoles to filter by PartyId
                // Purpose: Links orders to customers via OrderRoles to filter by partyId
                // Context: Assumes OrderRoles has PartyId and RoleTypeId, with "CUSTOMER" indicating the customer role
                if (!string.IsNullOrEmpty(request.PartyId))
                {
                    eligibleOrdersQuery = eligibleOrdersQuery
                        .Join(_context.OrderRoles,
                            order => order.OrderId,
                            orderRole => orderRole.OrderId,
                            (order, orderRole) => new { order, orderRole })
                        .Where(joined => joined.orderRole.PartyId == request.PartyId &&
                                         joined.orderRole.RoleTypeId == "BILL_TO_CUSTOMER")
                        .Select(joined => joined.order);
                }

                // Retrieve the list of approved sales order IDs.
                var eligibleOrderIds = await eligibleOrdersQuery
                    .Select(o => new ApprovedSalesOrderDto
                    {
                        OrderId = o.OrderId,
                        OrderStatus = o.StatusId,
                        OrderDescription =
                            $"{o.OrderId}  -  {(o.OrderDate.HasValue ? o.OrderDate.Value.ToString("dd/MM/yyyy HH:mm") : "No Date Available")}"
                    })
                    .ToListAsync(cancellationToken);

                return Result<List<ApprovedSalesOrderDto>>.Success(eligibleOrderIds);
            }
            catch (Exception ex)
            {
                return Result<List<ApprovedSalesOrderDto>>.Failure($"Exception: {ex.Message}");
            }
        }
    }
}