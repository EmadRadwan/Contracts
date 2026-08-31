using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Persistence;

namespace Application.Auditing;

/// <summary>
/// Feeds both the change-log grid and the per-record History tab. The History tab filters on
/// ChangedEntityName + PkCombinedValueText, which the ENTITY_AUDIT_LOG_RECORD index covers.
/// </summary>
public class ListEntityAuditLogs
{
    public class Query : IRequest<IQueryable<EntityAuditLogRecord>>
    {
        public ODataQueryOptions<EntityAuditLogRecord> Options { get; set; } = null!;
    }

    public class Handler : IRequestHandler<Query, IQueryable<EntityAuditLogRecord>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public Task<IQueryable<EntityAuditLogRecord>> Handle(Query request, CancellationToken cancellationToken)
        {
            var changes = _context.EntityAuditLogs
                .OrderByDescending(x => x.ChangedDate)
                .Select(x => new EntityAuditLogRecord
                {
                    AuditHistorySeqId = x.AuditHistorySeqId,
                    ChangedEntityName = x.ChangedEntityName,
                    ChangedFieldName = x.ChangedFieldName,
                    PkCombinedValueText = x.PkCombinedValueText,
                    OldValueText = x.OldValueText,
                    NewValueText = x.NewValueText,
                    ChangedDate = x.ChangedDate,
                    ChangedByInfo = x.ChangedByInfo,
                    ChangedSessionInfo = x.ChangedSessionInfo
                });

            return Task.FromResult(changes);
        }
    }
}
