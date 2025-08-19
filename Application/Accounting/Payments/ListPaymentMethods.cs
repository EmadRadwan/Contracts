using AutoMapper;
using MediatR;
using Persistence;

namespace Application.Accounting.Payments;


public class ListPaymentMethods
{
    public class Query : IRequest<Result<List<PaymentMethodDto>>>
    {
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
            var query = from pm in _context.PaymentMethods
                select new PaymentMethodDto
                {
                    PaymentMethodId = pm.PaymentMethodId,
                    PaymentMethodTypeId = pm.PaymentMethodTypeId,
                    PartyId = pm.PartyId,
                    GlAccountId = pm.GlAccountId,
                    FinAccountId = pm.FinAccountId,
                    Description = pm.Description ?? pm.PaymentMethodTypeId
                };


            List<PaymentMethodDto> results = query.ToList();

            results.Insert(0, new PaymentMethodDto
            {
                PaymentMethodId = null, // or 0 if using integers
                Description = ""
            });


            return Result<List<PaymentMethodDto>>.Success(results);
        }
    }
}