using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.RoleTypes;

public class List
{
    public class Query : IRequest<Result<List<RoleTypeDto>>>
    {
    }


    public class Handler : IRequestHandler<Query, Result<List<RoleTypeDto>>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<List<RoleTypeDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = _context.RoleTypes
                // .Where(z => z.IncludeInFilter)
                .OrderBy(x => x.Description)
                .ProjectTo<RoleTypeDto>(_mapper.ConfigurationProvider)
                .AsQueryable();


            var roleTypes = await query
                .ToListAsync();

            return Result<List<RoleTypeDto>>.Success(roleTypes);
        }
    }
}