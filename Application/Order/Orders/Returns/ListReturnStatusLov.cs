using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Order.Orders.Returns
{
    public class GetReturnStatusItemsQuery : IRequest<List<StatusItemDto>>
    {
        public string ReturnId { get; set; }
        public string ReturnHeaderType { get; set; } // "V" or "C"
    }

    public class StatusItemDto
    {
        public string StatusId { get; set; }
        public string Description { get; set; }
    }

    public class GetReturnStatusItemsQueryHandler : IRequestHandler<GetReturnStatusItemsQuery, List<StatusItemDto>>
    {
        private readonly DataContext _context;

        public GetReturnStatusItemsQueryHandler(DataContext context)
        {
            _context = context;
        }

        public async Task<List<StatusItemDto>> Handle(GetReturnStatusItemsQuery request, CancellationToken cancellationToken)
        {
            // REFACTOR: Validate returnHeaderType and set status type based on database StatusItems
            if (request.ReturnHeaderType != "V" && request.ReturnHeaderType != "C")
            {
                throw new ArgumentException("Invalid returnHeaderType. Must be 'V' or 'C'.");
            }
            var statusTypeId = request.ReturnHeaderType == "V" ? "PORDER_RETURN_STTS" : "ORDER_RETURN_STTS";
            var initialStatusId = request.ReturnHeaderType == "V" ? "SUP_RETURN_REQUESTED" : "RETURN_REQUESTED";

            if (!string.IsNullOrEmpty(request.ReturnId))
            {
                // REFACTOR: Fetch return header and validate type
                var returnHeader = await _context.ReturnHeaders
                    .FirstOrDefaultAsync(r => r.ReturnId == request.ReturnId, cancellationToken);
                if (returnHeader == null)
                {
                    throw new ArgumentException("Return not found");
                }

                var expectedType = request.ReturnHeaderType == "V" ? "VENDOR_RETURN" : "CUSTOMER_RETURN";
                if (returnHeader.ReturnHeaderTypeId != expectedType)
                {
                    throw new ArgumentException($"Return type mismatch. Expected {expectedType}, got {returnHeader.ReturnHeaderTypeId}");
                }

                // REFACTOR: Fetch current status from StatusItems
                var currentStatus = await _context.StatusItems
                    .Where(s => s.StatusId == returnHeader.StatusId && s.StatusTypeId == statusTypeId)
                    .Select(s => new StatusItemDto
                    {
                        StatusId = s.StatusId,
                        Description = $"{s.Description} (Current)"
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                // REFACTOR: Fetch valid next statuses from StatusValidChanges, filtered by StatusTypeId
                var nextStatuses = await (from svc in _context.StatusValidChanges
                                         join si in _context.StatusItems on svc.StatusIdTo equals si.StatusId
                                         where svc.StatusId == returnHeader.StatusId && si.StatusTypeId == statusTypeId
                                         select new StatusItemDto
                                         {
                                             StatusId = si.StatusId,
                                             Description = $"{svc.TransitionName} ({si.Description})"
                                         })
                    .OrderBy(s => s.StatusId) // Optional: Order by StatusId or use SEQUENCE_ID
                    .ToListAsync(cancellationToken);

                var result = new List<StatusItemDto>();
                if (currentStatus != null)
                {
                    result.Add(currentStatus);
                }
                result.AddRange(nextStatuses);
                return result;
            }
            else
            {
                // REFACTOR: Fetch initial status for new returns from StatusItems
                var initialStatus = await _context.StatusItems
                    .Where(s => s.StatusId == initialStatusId && s.StatusTypeId == statusTypeId)
                    .Select(s => new StatusItemDto
                    {
                        StatusId = s.StatusId,
                        Description = s.Description
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (initialStatus == null)
                {
                    throw new InvalidOperationException($"Initial status {initialStatusId} not found for {statusTypeId}");
                }

                return new List<StatusItemDto> { initialStatus };
            }
        }
    }
}