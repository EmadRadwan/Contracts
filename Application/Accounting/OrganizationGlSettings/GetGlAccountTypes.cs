using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Shipments.OrganizationGlSettings;

public class GetGlAccountTypes
{
    public class Query : IRequest<Result<List<GlAccountTypeDto>>>
    {
    }

    public class Handler : IRequestHandler<Query, Result<List<GlAccountTypeDto>>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<Result<List<GlAccountTypeDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            try
            {
                var glAccountTypes = await (from glat in _context.GlAccountTypes
                    select new GlAccountTypeDto
                    {
                        GlAccountTypeId = glat.GlAccountTypeId,
                        Description = glat.Description
                    }).ToListAsync(cancellationToken);

                return Result<List<GlAccountTypeDto>>.Success(glAccountTypes);
            }
            catch (Exception ex)
            {
                // Handle exception and return an error result
                return Result<List<GlAccountTypeDto>>.Failure(ex.Message);
            }
        }
    }
}