using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Order.Orders.Returns
{
    public class GetPartyOrders
    {
        public class Query : IRequest<Result<List<OrderHeaderItemAndRolesDto>>>
        {
            public string ReturnId { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<List<OrderHeaderItemAndRolesDto>>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<List<OrderHeaderItemAndRolesDto>>> Handle(Query request,
                CancellationToken cancellationToken)
            {
                // 1. Validate ReturnId
                if (string.IsNullOrEmpty(request.ReturnId))
                    return Result<List<OrderHeaderItemAndRolesDto>>.Failure("ReturnId is required.");

                // 2. Retrieve ReturnHeader
                var returnHeader = await _context.ReturnHeaders
                    .FirstOrDefaultAsync(rh => rh.ReturnId == request.ReturnId, cancellationToken);

                if (returnHeader == null)
                    return Result<List<OrderHeaderItemAndRolesDto>>.Failure("ReturnHeader not found.");

                // 3. Determine roleTypeId and partyId
                string roleTypeId = "PLACING_CUSTOMER";
                string partyId = returnHeader.FromPartyId;

                if (returnHeader.ReturnHeaderTypeId == "VENDOR_RETURN")
                {
                    roleTypeId = "BILL_FROM_VENDOR";
                    partyId = returnHeader.ToPartyId;
                }

                // 4. Retrieve PartyOrders
                var partyOrders = await (from ot in _context.OrderRoles
                        join oh in _context.OrderHeaders on ot.OrderId equals oh.OrderId
                        join oi in _context.OrderItems on oh.OrderId equals oi.OrderId
                        where ot.RoleTypeId == roleTypeId
                              && ot.PartyId == partyId
                              && oi.StatusId == "ITEM_COMPLETED"
                        orderby oh.OrderId
                        select new OrderHeaderItemAndRolesDto
                        {
                            OrderId = oh.OrderId,
                            OrderDate = (DateTime)oh.OrderDate,
                            PartyId = ot.PartyId,
                            RoleTypeId = ot.RoleTypeId,
                            OrderTypeId = oh.OrderTypeId,
                            StatusId = oh.StatusId,
                            ProductId = oi.ProductId,
                            Quantity = (decimal)oi.Quantity,
                            UnitPrice = (decimal)oi.UnitPrice,
                            ItemDescription = oi.ItemDescription,
                            OrderItemSeqId = oi.OrderItemSeqId
                        })
                    .Distinct()
                    .ToListAsync(cancellationToken);

                // 5. Return the result
                return Result<List<OrderHeaderItemAndRolesDto>>.Success(partyOrders);
            }
        }
    }
}