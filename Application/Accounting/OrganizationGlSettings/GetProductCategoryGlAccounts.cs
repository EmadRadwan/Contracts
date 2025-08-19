using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Shipments.OrganizationGlSettings
{
    public class GetProductCategoryGlAccounts
    {
        public class Query : IRequest<Result<List<GetProductCategoryGlAccountDto>>>
        {
            public string CompanyId { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<List<GetProductCategoryGlAccountDto>>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _mapper = mapper;
                _context = context;
            }

            public async Task<Result<List<GetProductCategoryGlAccountDto>>> Handle(Query request,
                CancellationToken cancellationToken)
            {
                var productCategoryGlAccounts = await (from pcga in _context.ProductCategoryGlAccounts
                    join a in _context.GlAccounts
                        on pcga.GlAccountId equals a.GlAccountId
                    join pc in _context.ProductCategories
                        on pcga.ProductCategoryId equals pc.ProductCategoryId
                    where pcga.OrganizationPartyId == request.CompanyId
                    select new GetProductCategoryGlAccountDto
                    {
                        ProductCategoryId = pc.ProductCategoryId,
                        ProductCategoryDescription = pc.CategoryName, // Assuming CategoryName is the description field
                        GlAccountId = pcga.GlAccountId,
                        GlAccountName = pcga.GlAccountId + " - " + a.AccountName // Concatenate GlAccountId and GlAccountName
                    }).ToListAsync(cancellationToken);

                return Result<List<GetProductCategoryGlAccountDto>>.Success(productCategoryGlAccounts!);
            }
        }
    }
}