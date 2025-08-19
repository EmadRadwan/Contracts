using Application.Shipments.Payments;
using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Accounting.Payments;


public class GetPaymentApplicationsLov
{
    public class PaymentsEnvelope
    {
        public List<PaymentDto2> Payments { get; set; }
        public int PaymentsCount { get; set; }
    }

    public class Query : IRequest<Result<PaymentsEnvelope>>
    {
        public PaymentsLovParam? Params { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<PaymentsEnvelope>>
    {
        private readonly DataContext _context;
        private readonly ILogger<Handler> _logger;
        private readonly IMapper _mapper;
        private readonly IUserAccessor _userAccessor;

        public Handler(DataContext context, IMapper mapper, IUserAccessor userAccessor, ILogger<Handler> logger)
        {
            _mapper = mapper;
            _context = context;
            _userAccessor = userAccessor;
            _logger = logger;
        }

        public async Task<Result<PaymentsEnvelope>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = from pyt in _context.Payments
                join ptt in _context.PaymentTypes on pyt.PaymentTypeId equals ptt.PaymentTypeId
                join sts in _context.StatusItems on pyt.StatusId equals sts.StatusId
                join pty in _context.Parties on pyt.PartyIdFrom equals pty.PartyId
                join ptyto in _context.Parties on pyt.PartyIdTo equals ptyto.PartyId
                where pyt.PartyIdFrom == request.Params!.PartyId
                select new PaymentDto2
                {
                    PaymentId = pyt.PaymentId,
                    PaymentTypeId = pyt.PaymentTypeId,
                    PaymentTypeDescription = ptt.Description,
                    PartyIdFrom = new PaymentPartyDto
                    {
                        FromPartyId = pty.PartyId,
                        FromPartyName = pty.Description
                    },
                    PartyId = new PaymentPartyDto
                    {
                        FromPartyId = ptyto.PartyId,
                        FromPartyName = ptyto.Description
                    },
                    PaymentDate = pyt.EffectiveDate,
                    StatusDescription = sts.Description,
                    AllowSubmit = false
                };

            var result = await query.ToListAsync();

            var paymentsEnvelop = new PaymentsEnvelope
            {
                Payments = result,
                PaymentsCount = query.Count()
            };


            return Result<PaymentsEnvelope>.Success(paymentsEnvelop);
        }
    }
}