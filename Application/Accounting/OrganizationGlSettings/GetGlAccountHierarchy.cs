using AutoMapper;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Shipments.OrganizationGlSettings;

public class GetGlAccountHierarchy
{
    public class Query : IRequest<Result<List<GlAccountHierarchyView>>>
    {
    }

    public class Handler : IRequestHandler<Query, Result<List<GlAccountHierarchyView>>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<Result<List<GlAccountHierarchyView>>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            var glAccountHierarchyViews = await _context.GlAccountHierarchyView.ToListAsync(cancellationToken);

            return Result<List<GlAccountHierarchyView>>.Success(glAccountHierarchyViews);
        }
    }
}