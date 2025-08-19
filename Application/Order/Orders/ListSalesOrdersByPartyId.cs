using AutoMapper;
using MediatR;
using Persistence;

namespace Application.Order.Orders;

public class ListSalesOrdersByPartyId
{
    public class Query : IRequest<Result<List<OrderByPartyIdDto>>>
    {
        public string PartyId { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<List<OrderByPartyIdDto>>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<Result<List<OrderByPartyIdDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = from ord in _context.OrderHeaders
                join orole in _context.OrderRoles on ord.OrderId equals orole.OrderId
                join sts in _context.StatusItems on ord.StatusId equals sts.StatusId
                join pty in _context.Parties on orole.PartyId equals pty.PartyId
                where orole.PartyId == request.PartyId && orole.RoleTypeId == "PLACING_CUSTOMER" &&
                      ord.StatusId == "ORDER_COMPLETED"
                orderby ord.OrderDate descending
                select new OrderByPartyIdDto
                {
                    OrderId = ord.OrderId,
                    OrderDescription = ord.OrderId + "  /  " + ord.OrderDate
                };


            var results = query.ToList();


            return Result<List<OrderByPartyIdDto>>.Success(results);
        }
    }
}