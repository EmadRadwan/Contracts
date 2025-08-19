using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Shipments.OrganizationGlSettings
{
    public class GetPaymentTypeGlAccountTypes
    {
        public class Query : IRequest<Result<List<GetPaymentGlAccountTypeMapDto>>>
        {
            public string CompanyId { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<List<GetPaymentGlAccountTypeMapDto>>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _mapper = mapper;
                _context = context;
            }

            public async Task<Result<List<GetPaymentGlAccountTypeMapDto>>> Handle(Query request,
                CancellationToken cancellationToken)
            {
                var paymentGlAccountTypeMaps = await (from pgatm in _context.PaymentGlAccountTypeMaps
                    join gat in _context.GlAccountTypes
                        on pgatm.GlAccountTypeId equals gat.GlAccountTypeId
                    join pt in _context.PaymentTypes
                        on pgatm.PaymentTypeId equals pt.PaymentTypeId
                    where pgatm.OrganizationPartyId == request.CompanyId
                    select new GetPaymentGlAccountTypeMapDto
                    {
                        PaymentTypeId = pgatm.PaymentTypeId,
                        PaymentTypeDescription = pt.Description,
                        GlAccountTypeId = pgatm.GlAccountTypeId,
                        GlAccountTypeName = gat.Description // Concatenate GlAccountTypeId and Description
                    }).ToListAsync(cancellationToken);

                return Result<List<GetPaymentGlAccountTypeMapDto>>.Success(paymentGlAccountTypeMaps!);
            }
        }
    }
}