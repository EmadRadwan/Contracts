using Application.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.CRM.Leads.Assignment;

/// <summary>
/// Returns the full ownership history of a Lead, newest first.
///
/// Because reassignment closes the previous LEAD_OWNER row rather than deleting
/// it, the history is simply every LEAD_OWNER relationship for this lead.
/// </summary>
public class ListLeadAssignmentHistory
{
    public record Query : IRequest<Result<List<LeadAssignmentHistoryDto>>>
    {
        public string LeadPartyId { get; init; } = null!;
    }

    public class Handler : IRequestHandler<Query, Result<List<LeadAssignmentHistoryDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<LeadAssignmentHistoryDto>>> Handle(Query request, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request.LeadPartyId))
                return Result<List<LeadAssignmentHistoryDto>>.Failure("Lead is required");

            var history = await (
                from pr in _context.PartyRelationships
                join owner in _context.Parties on pr.PartyIdFrom equals owner.PartyId into owners
                from owner in owners.DefaultIfEmpty()
                where pr.PartyIdTo == request.LeadPartyId
                   && pr.PartyRelationshipTypeId == LeadAssignmentConstants.RelationshipTypeId
                select new LeadAssignmentHistoryDto
                {
                    OwnerPartyId = pr.PartyIdFrom,
                    OwnerName = owner != null ? owner.Description : null,
                    FromDate = pr.FromDate,
                    ThruDate = pr.ThruDate,
                    Comments = pr.Comments,
                    AssignedByUserLogin = pr.CreatedByUserLogin,
                    IsCurrent = pr.ThruDate == null
                })
                .OrderByDescending(x => x.FromDate)
                .ToListAsync(ct);

            return Result<List<LeadAssignmentHistoryDto>>.Success(history);
        }
    }
}
