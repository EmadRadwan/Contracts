using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Parties.Parties;

public class GetContractorsAndSuppliersLov
{
    public class ContractorsEnvelop
    {
        public List<PartyFromPartyIdDto>? Parties { get; set; }
        public int PartyCount { get; set; }
    }

    public class PartyFromPartyIdDto
    {
        public string fromPartyId { get; set; }
        public string fromPartyName { get; set; }
    }

    public class Query : IRequest<Result<ContractorsEnvelop>>
    {
        public PartyLovParams? Params { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<ContractorsEnvelop>>
    {
        private readonly DataContext _context;
        private readonly ILogger<Handler> _logger;
        private readonly IUserAccessor _userAccessor;

        public Handler(DataContext context, IUserAccessor userAccessor, ILogger<Handler> logger)
        {
            _context = context;
            _userAccessor = userAccessor;
            _logger = logger;
        }

        public async Task<Result<ContractorsEnvelop>> Handle(Query request, CancellationToken cancellationToken)
        {
            // REFACTOR: Validate input
            // Purpose: Prevent null reference exceptions
            // Context: Ensure request parameters are valid
            if (request?.Params == null)
            {
                _logger.LogWarning("Invalid request: Params is null");
                return Result<ContractorsEnvelop>.Failure("Invalid request parameters.");
            }

            var query = _context.Parties
                .Where(x => x.MainRole == "Contractor" || x.MainRole == "Supplier")
                .AsQueryable();

            // REFACTOR: Update search logic
            // Purpose: Allow searching by both fromPartyId and fromPartyName
            // Context: Modified to include PartyId in search conditions
            if (!string.IsNullOrEmpty(request.Params.SearchTerm))
            {
                var lowerCaseSearchTerm = request.Params.SearchTerm.Trim().ToLower();
                query = query.Where(p => 
                    p.Description!.ToLower().Contains(lowerCaseSearchTerm) ||
                    p.PartyId.ToLower().Contains(lowerCaseSearchTerm));
            }

            var total = await query.CountAsync(cancellationToken);

            // REFACTOR: Remove AutoMapper and manually map to DTO
            // Purpose: Eliminate AutoMapper dependency for simpler mapping
            // Context: Directly select fields to create PartyFromPartyIdDto
            var parties = await query
                .OrderBy(x => x.Description)
                .Skip(request.Params.Skip)
                .Take(request.Params.PageSize)
                .Select(p => new PartyFromPartyIdDto
                {
                    fromPartyId = p.PartyId,
                    fromPartyName = p.Description
                })
                .ToListAsync(cancellationToken);

            var partyEnvelop = new ContractorsEnvelop
            {
                Parties = parties,
                PartyCount = total
            };

            return Result<ContractorsEnvelop>.Success(partyEnvelop);
        }
    }
}