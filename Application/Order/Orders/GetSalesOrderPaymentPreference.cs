using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Order.Orders;

public class GetSalesOrderPaymentPreference
{
    public class Query : IRequest<Result<List<OrderPaymentPreferenceDto>>>
    {
        public string OrderId { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<List<OrderPaymentPreferenceDto>>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<Result<List<OrderPaymentPreferenceDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            
            var result = await (from opp in _context.OrderPaymentPreferences
                .Where(o => o.OrderId == request.OrderId)
                join status in _context.StatusItems on opp.StatusId equals status.StatusId
                join order in _context.OrderHeaders on request.OrderId equals order.OrderId
                join uoms in _context.Uoms on order.CurrencyUom equals uoms.UomId
                join pmt in _context.PaymentMethodTypes on opp.PaymentMethodTypeId equals pmt.PaymentMethodTypeId
                select new OrderPaymentPreferenceDto
                {
                    OrderId = request.OrderId,
                    StatusId = opp.StatusId,
                    StatusDescription = status.Description,
                    PaymentMethodTypeId = opp.PaymentMethodTypeId,
                    PaymentMethodTypeDescription = pmt.Description,
                    MaxAmount = opp.MaxAmount,
                    UomId = order.CurrencyUom,
                    UomDescription = uoms.Description
                }).ToListAsync();

            return Result<List<OrderPaymentPreferenceDto>>.Success(result);
        }
    }
}