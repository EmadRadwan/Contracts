using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Persistence;

namespace Application.Auditing;

/// <summary>
/// Feeds the Audit Trail grid. Returns IQueryable so OData applies filtering, sorting and paging
/// in SQL — the table is expected to grow large, so nothing is materialised here.
/// </summary>
public class ListAuditActivities
{
    public class Query : IRequest<IQueryable<AuditActivityRecord>>
    {
        public ODataQueryOptions<AuditActivityRecord> Options { get; set; } = null!;
    }

    public class Handler : IRequestHandler<Query, IQueryable<AuditActivityRecord>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public Task<IQueryable<AuditActivityRecord>> Handle(Query request, CancellationToken cancellationToken)
        {
            var activities = _context.AuditActivities
                .OrderByDescending(x => x.StartedAt)
                .Select(x => new AuditActivityRecord
                {
                    ActivityId = x.ActivityId,
                    StartedAt = x.StartedAt,
                    UserName = x.UserName,
                    UserId = x.UserId,
                    RequestName = x.RequestName,
                    RequestPath = x.RequestPath,
                    HttpMethod = x.HttpMethod,
                    ClientIpAddress = x.ClientIpAddress,
                    IsSuccess = x.IsSuccess,
                    ErrorMessage = x.ErrorMessage,
                    ExceptionType = x.ExceptionType,
                    DurationMs = x.DurationMs,
                    CorrelationId = x.CorrelationId,
                    RequestJson = x.RequestJson
                });

            return Task.FromResult(activities);
        }
    }
}
