using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Parties.Parties;

public class ListRoles
{
    public class Query : IRequest<Result<List<RoleDto>>>
    {
    }

    public class Handler : IRequestHandler<Query, Result<List<RoleDto>>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<Result<List<RoleDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            
            var query = await (from r in _context.RoleTypes
                select new RoleDto
                {
                    RoleTypeId = r.RoleTypeId,
                    RoleName = r.Description
                }).ToListAsync(cancellationToken);

            return Result<List<RoleDto>>.Success(query);
        }
    }
}