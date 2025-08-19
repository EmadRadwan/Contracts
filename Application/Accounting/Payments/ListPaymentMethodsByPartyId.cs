using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.Payments;


public class ListPaymentMethodsByPartyId
{
    public class Query : IRequest<Result<List<PaymentMethodDto>>>
    {
        public string PartyId { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<List<PaymentMethodDto>>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<Result<List<PaymentMethodDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = await (from pm in _context.PaymentMethods
                    .Where(pm => pm.PartyId == request.PartyId)
                select new PaymentMethodDto
                {
                    PaymentMethodId = pm.PaymentMethodId,
                    PaymentMethodTypeId = pm.PaymentMethodTypeId,
                    PartyId = pm.PartyId,
                    GlAccountId = pm.GlAccountId,
                    FinAccountId = pm.FinAccountId,
                    Description = pm.Description ?? pm.PaymentMethodTypeId
                }).ToListAsync();


            return Result<List<PaymentMethodDto>>.Success(query);
        }
    }
}