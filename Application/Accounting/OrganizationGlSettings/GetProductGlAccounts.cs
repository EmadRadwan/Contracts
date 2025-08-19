using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Shipments.OrganizationGlSettings
{
    public class GetProductGlAccounts
    {
        public class Query : IRequest<Result<List<GetProductGlAccountDto>>>
        {
            public string CompanyId { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<List<GetProductGlAccountDto>>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _mapper = mapper;
                _context = context;
            }

            public async Task<Result<List<GetProductGlAccountDto>>> Handle(Query request,
                CancellationToken cancellationToken)
            {
                var productGlAccounts = await (from pga in _context.ProductGlAccounts
                    join a in _context.GlAccounts
                        on pga.GlAccountId equals a.GlAccountId
                    join p in _context.Products
                        on pga.ProductId equals p.ProductId
                    where pga.OrganizationPartyId == request.CompanyId
                    select new GetProductGlAccountDto
                    {
                        ProductId = p.ProductId,
                        ProductDescription = p.ProductName, // Assuming ProductName is the description field
                        GlAccountId = pga.GlAccountId,
                        GlAccountName = pga.GlAccountId + " - " + a.AccountName // Concatenate GlAccountId and GlAccountName
                    }).ToListAsync(cancellationToken);

                return Result<List<GetProductGlAccountDto>>.Success(productGlAccounts!);
            }
        }
    }
}