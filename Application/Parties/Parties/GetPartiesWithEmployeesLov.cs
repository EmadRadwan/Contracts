using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Parties.Parties;

public class GetPartiesWithEmployeesLov
{
    public class PartiesEnvelope
    {
        public List<PartyFromPartyIdDto> Parties { get; set; }
        public int PartyCount { get; set; }
    }

    public class Query : IRequest<Result<PartiesEnvelope>>
    {
        public PartyLovParams? Params { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<PartiesEnvelope>>
    {
        private readonly DataContext _context;
        private readonly ILogger<Handler> _logger;
        private readonly IMapper _mapper;
        private readonly IUserAccessor _userAccessor;


        public Handler(DataContext context, IMapper mapper, IUserAccessor userAccessor, ILogger<Handler> logger)
        {
            _mapper = mapper;
            _context = context;
            _userAccessor = userAccessor;
            _logger = logger;
        }

        public async Task<Result<PartiesEnvelope>> Handle(Query request, CancellationToken cancellationToken)
        {
            var queryByPhone = _context.PartyView.Select(x => new PartyFromPartyIdDto
            {
                FromPartyId = x.FromPartyId,
                FromPartyName = x.FromPartyName,
                FromPartyPhone = x.FromPartyPhone
            }).AsQueryable();

            var query = Enumerable.Empty<PartyFromPartyIdDto>().AsQueryable();

            var queryByName = _context.Parties
                .Where(x => x.MainRole == "CUSTOMER" || x.MainRole == "SUPPLIER" || x.MainRole == "EMPLOYEE")
                .Select(x => new PartyFromPartyIdDto
                {
                    FromPartyId = x.PartyId,
                    FromPartyName = x.Description,
                    FromPartyPhone = string.Empty
                })
                .AsQueryable();


            if (!string.IsNullOrEmpty(request.Params.SearchTerm))
            {
                var lowerCaseSearchTerm = request.Params.SearchTerm.Trim().ToLower();

                // Check if the search term is a numeric value (party number or phone number)
                if (int.TryParse(lowerCaseSearchTerm, out var partyNumber))
                    // Handle search by party number
                    query = queryByPhone.Where(p =>
                        p.FromPartyPhone != null && p.FromPartyPhone.Contains(partyNumber.ToString()));
                else
                    // Handle search by party name
                    query = queryByName.Where(p => p.FromPartyName.ToLower().Contains(lowerCaseSearchTerm));
            }
            else
            {
                // Handle the case when the searchTerm is empty
                query = queryByName;
            }


            var Partys = await query
                .OrderBy(x => x.FromPartyName)
                .Skip(request.Params.Skip)
                .Take(request.Params.PageSize)
                .ToListAsync();

            var PartyEnvelop = new PartiesEnvelope
            {
                Parties = Partys,
                PartyCount = query.Count()
            };


            return Result<PartiesEnvelope>.Success(PartyEnvelop);
        }
    }
}