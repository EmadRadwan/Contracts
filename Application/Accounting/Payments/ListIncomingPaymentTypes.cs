using AutoMapper;
using MediatR;
using Persistence;

namespace Application.Accounting.Payments;


public class ListIncomingPaymentTypes
{
    public class Query : IRequest<Result<List<PaymentTypeDto>>>
    {
        public string Language { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<List<PaymentTypeDto>>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<Result<List<PaymentTypeDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var language = request.Language;
            var allowedTypes = new List<string> { "CUSTOMER_DEPOSIT", "CUSTOMER_PAYMENT" };

            var paymentTypes = _context.PaymentTypes
                .Where(x => allowedTypes.Contains(x.PaymentTypeId))
                .Select(x => new PaymentTypeDto
                {
                    PaymentTypeId = x.PaymentTypeId,
                    Description = language == "ar" ? x.DescriptionArabic : x.Description
                })
                .ToList();


            return Result<List<PaymentTypeDto>>.Success(paymentTypes);
        }
    }
}