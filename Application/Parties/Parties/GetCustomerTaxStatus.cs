using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Parties.Parties;

public class GetCustomerTaxStatus
{
    public class Query : IRequest<Result<PartyTaxStatusDto>>
    {
        public string CustomerId { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<PartyTaxStatusDto>>
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

        public async Task<Result<PartyTaxStatusDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var customerTaxStatus = await _context.PartyTaxAuthInfos
                .Where(x => x.PartyId == request.CustomerId)
                .FirstOrDefaultAsync();

            // check if customer exists in table PartyTaxAuthInfo and return
            // the IsExempt status in a PartyTaxStatusDto object in all cases
            // if customer does not exist in table PartyTaxAuthInfo, return IsExempt = 'N
            // in a PartyTaxStatusDto object
            var partyTaxStatusDto = new PartyTaxStatusDto
            {
                PartyId = request.CustomerId,
                IsExempt = customerTaxStatus != null ? customerTaxStatus.IsExempt : "N"
            };


            return Result<PartyTaxStatusDto>.Success(partyTaxStatusDto);
        }
    }
}