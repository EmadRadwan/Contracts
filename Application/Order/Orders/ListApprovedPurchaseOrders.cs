using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Order.Orders;

public class ListApprovedPurchaseOrders
{
    public class Query : IRequest<Result<List<ApprovedPurchaseOrderDto>>>
    {
        public string? PartyId { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<List<ApprovedPurchaseOrderDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<ApprovedPurchaseOrderDto>>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            try
            {
                // REFACTOR: Query eligible purchase orders with initial filtering
                // Purpose: Filters orders by status and type, aligning with original logic
                // Context: Ensures only approved or completed purchase orders are included
                var eligibleOrdersQuery = _context.OrderHeaders
                    .Where(o => (o.StatusId == "ORDER_APPROVED" || o.StatusId == "ORDER_COMPLETED") &&
                                o.OrderTypeId == "PURCHASE_ORDER");

                // REFACTOR: Add join with OrderRoles to filter by PartyId
                // Purpose: Links orders to suppliers via OrderRoles to filter by partyId
                // Context: Assumes OrderRoles has PartyId and RoleTypeId, with "BILL_FROM_VENDOR" indicating the supplier role
                if (!string.IsNullOrEmpty(request.PartyId))
                {
                    eligibleOrdersQuery = eligibleOrdersQuery
                        .Join(_context.OrderRoles,
                            order => order.OrderId,
                            orderRole => orderRole.OrderId,
                            (order, orderRole) => new { order, orderRole })
                        .Where(joined => joined.orderRole.PartyId == request.PartyId &&
                                         joined.orderRole.RoleTypeId == "BILL_FROM_VENDOR")
                        .Select(joined => joined.order);
                }

                // REFACTOR: Retrieve and transform purchase orders into DTOs
                // Purpose: Maintains original DTO structure and sorting
                // Improvement: Adds explicit ordering and consistent date formatting
                var eligibleOrders = await eligibleOrdersQuery
                    .OrderByDescending(o => o.OrderDate)
                    .Select(o => new ApprovedPurchaseOrderDto
                    {
                        OrderId = o.OrderId,
                        OrderStatus = o.StatusId,
                        OrderDescription = o.OrderId + " - " + (o.OrderDate.HasValue
                            ? o.OrderDate.Value.ToString("dd/MM/yyyy HH:mm")
                            : "No Date Available")
                    })
                    .ToListAsync(cancellationToken);

                // REFACTOR: Add error handling with try-catch
                // Purpose: Catches and reports exceptions, improving robustness
                // Context: Aligns with ListApprovedSalesOrders for consistent error handling
                return Result<List<ApprovedPurchaseOrderDto>>.Success(eligibleOrders);
            }
            catch (Exception ex)
            {
                return Result<List<ApprovedPurchaseOrderDto>>.Failure($"Exception: {ex.Message}");
            }
        }
    }
}