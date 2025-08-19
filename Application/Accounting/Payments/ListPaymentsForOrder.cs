using AutoMapper;
using MediatR;
using Persistence;

namespace Application.Accounting.Payments;


public class ListPaymentsForOrder
{
    public class Query : IRequest<Result<List<PaymentDto>>>
    {
        public string OrderId { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<List<PaymentDto>>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<Result<List<PaymentDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = from pyt in _context.Payments
                join ptt in _context.PaymentTypes on pyt.PaymentTypeId equals ptt.PaymentTypeId
                join sts in _context.StatusItems on pyt.StatusId equals sts.StatusId
                //join pty in _context.Parties on pyt.PartyIdFrom equals pty.PartyId
                join opp in _context.OrderPaymentPreferences on pyt.PaymentPreferenceId equals opp
                    .OrderPaymentPreferenceId
                where opp.OrderId == request.OrderId
                select new PaymentDto
                {
                    PaymentId = pyt.PaymentId,
                    PaymentTypeId = pyt.PaymentTypeId,
                    PaymentTypeDescription = ptt.Description,
                    PaymentMethodTypeId = opp.PaymentMethodTypeId,
                    PaymentDate = pyt.EffectiveDate,
                    StatusDescription = sts.Description,
                    Amount = pyt.Amount
                };


            List<PaymentDto> results = query.ToList();


            return Result<List<PaymentDto>>.Success(results);
        }
    }
}