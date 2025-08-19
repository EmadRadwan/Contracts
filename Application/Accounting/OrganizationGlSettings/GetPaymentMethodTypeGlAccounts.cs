using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Shipments.OrganizationGlSettings
{
    public class GetPaymentMethodTypeGlAccounts
    {
        public class Query : IRequest<Result<List<GetPaymentMethodTypeGlAccountDto>>>
        {
            public string CompanyId { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<List<GetPaymentMethodTypeGlAccountDto>>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _mapper = mapper;
                _context = context;
            }

            public async Task<Result<List<GetPaymentMethodTypeGlAccountDto>>> Handle(Query request,
                CancellationToken cancellationToken)
            {
                var paymentMethodTypeGlAccounts = await (from pmtga in _context.PaymentMethodTypeGlAccounts
                    join a in _context.GlAccounts
                        on pmtga.GlAccountId equals a.GlAccountId
                    join pmt in _context.PaymentMethodTypes
                        on pmtga.PaymentMethodTypeId equals pmt.PaymentMethodTypeId
                    where pmtga.OrganizationPartyId == request.CompanyId
                    select new GetPaymentMethodTypeGlAccountDto
                    {
                        PaymentMethodTypeId = pmt.PaymentMethodTypeId,
                        PaymentMethodTypeDescription = pmt.Description, // Assuming Description is the description field
                        GlAccountId = pmtga.GlAccountId,
                        GlAccountName = pmtga.GlAccountId + " - " + a.AccountName // Concatenate GlAccountId and GlAccountName
                    }).ToListAsync(cancellationToken);

                return Result<List<GetPaymentMethodTypeGlAccountDto>>.Success(paymentMethodTypeGlAccounts!);
            }
        }
    }
}