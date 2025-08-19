using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Application.Order.Orders.Returns;
using Microsoft.Extensions.Logging;
using Application.Interfaces;
using AutoMapper;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Order.Orders.Returns
{
    // REFACTOR: Consolidate Query and Handler into a single GetReturnById class
    // Purpose: Combine query definition and handling logic for fetching a return by ID
    // Why: Simplifies structure by keeping related logic in one class, as requested
    public class GetReturnById : IRequest<ReturnRecord>
    {
        public string ReturnId { get; set; }

        // REFACTOR: Implement handler logic within GetReturnById class
        // Purpose: Execute LINQ query to fetch a single return with joins
        // Why: Adapts provided LINQ query to return a single record for returnId
        public class Handler : IRequestHandler<GetReturnById, ReturnRecord>
        {
            private readonly DataContext _context;
            private readonly ILogger<Handler> _logger;
            private readonly IMapper _mapper;
            private readonly IUserAccessor _userAccessor;

            public Handler(DataContext context, IMapper mapper, IUserAccessor userAccessor, ILogger<Handler> logger)
            {
                _context = context;
                _mapper = mapper;
                _userAccessor = userAccessor;
                _logger = logger;
            }

            public async Task<ReturnRecord> Handle(GetReturnById request, CancellationToken cancellationToken)
            {
                // REFACTOR: Modify LINQ query to filter by returnId and return a single record
                // Purpose: Fetch a single return with necessary joins for EditReturn form
                // Why: Ensures efficient data retrieval for specific return ID
                var query = from ret in _context.ReturnHeaders
                            join pty in _context.Parties on ret.FromPartyId equals pty.PartyId
                            join pty2 in _context.Parties on ret.ToPartyId equals pty2.PartyId into toPartyJoin
                            from pty2 in toPartyJoin.DefaultIfEmpty()
                            join rett in _context.ReturnHeaderTypes on ret.ReturnHeaderTypeId equals rett.ReturnHeaderTypeId
                            join sts in _context.StatusItems on ret.StatusId equals sts.StatusId
                            where ret.ReturnId == request.ReturnId
                            select new ReturnRecord
                            {
                                ReturnId = ret.ReturnId,
                                FromPartyId = new OrderPartyDto
                                {
                                    FromPartyId = pty.PartyId,
                                    FromPartyName = pty.Description ?? string.Empty
                                },
                                FromPartyName = pty.Description,
                                ToPartyId = new OrderPartyDto
                                {
                                    FromPartyId = pty2.PartyId,
                                    FromPartyName = pty2.Description ?? string.Empty
                                },
                                ToPartyName = pty2 != null ? pty2.Description ?? string.Empty : string.Empty,
                                EntryDate = ret.EntryDate,
                                StatusId = sts.StatusId,
                                StatusDescription = sts.Description ?? string.Empty,
                                ReturnHeaderTypeId = rett.ReturnHeaderTypeId,
                                ReturnHeaderTypeDescription = rett.Description ?? string.Empty,
                                DestinationFacilityId = ret.DestinationFacilityId,
                                CurrencyUomId = ret.CurrencyUomId,
                                NeedsInventoryReceive = ret.NeedsInventoryReceive
                            };

                var result = await query.FirstOrDefaultAsync(cancellationToken);
                if (result == null)
                {
                    _logger.LogWarning("Return with ID {ReturnId} not found", request.ReturnId);
                }

                return result;
            }
        }
    }
}