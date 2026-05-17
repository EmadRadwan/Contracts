using Application.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Parties.Parties;

public class ListRoleTypes
{
    public class Query : IRequest<Result<List<RoleDto>>>
    {
        public string? SearchTerm { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<List<RoleDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<RoleDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = _context.RoleTypes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                query = query.Where(r => r.Description.Contains(request.SearchTerm));

            var roles = await query
                .Select(r => new RoleDto
                {
                    RoleTypeId = r.RoleTypeId,
                    RoleName = r.Description
                })
                .ToListAsync(cancellationToken);

            return Result<List<RoleDto>>.Success(roles);
        }
    }
}