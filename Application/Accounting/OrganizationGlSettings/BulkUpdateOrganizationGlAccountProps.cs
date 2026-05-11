using Application.Core;
using MediatR;
using Persistence;
using Microsoft.EntityFrameworkCore;

namespace Application.Accounting.OrganizationGlSettings;

public class BulkUpdateOrganizationGlAccountProps
{
    public class Command : IRequest<Results<Unit>>
    {
        public List<UpdateDto> Updates { get; set; } = new();
    }

    public class UpdateDto
    {
        public string GlAccountId { get; set; } = null!;
        public string? GlReportId { get; set; }
        public string? GlClassCourseId { get; set; }
        public string? GlSubClassId { get; set; }
        public string? GlSubClass2Id { get; set; }
        public string? GlAccountCourseLabelId { get; set; }
    }

    public class Handler : IRequestHandler<Command, Results<Unit>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Results<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            if (request.Updates == null || !request.Updates.Any())
                return Results<Unit>.Success(Unit.Value);

            var accountIds = request.Updates.Select(u => u.GlAccountId).ToList();
            var accounts = await _context.GlAccounts
                .Where(a => accountIds.Contains(a.GlAccountId))
                .ToListAsync(cancellationToken);

            foreach (var update in request.Updates)
            {
                var account = accounts.FirstOrDefault(a => a.GlAccountId == update.GlAccountId);
                if (account != null)
                {
                    account.GlReportId = update.GlReportId;
                    account.GlClassCourseId = update.GlClassCourseId;
                    account.GlSubClassId = update.GlSubClassId;
                    account.GlSubClass2Id = update.GlSubClass2Id;
                    account.GlAccountCourseLabelId = update.GlAccountCourseLabelId;
                    account.LastUpdatedStamp = DateTime.UtcNow;
                }
            }

            var result = await _context.SaveChangesAsync(cancellationToken) > 0;

            if (!result) return Results<Unit>.Failure("Failed to update GL accounts in bulk", "BULK_UPDATE_FAILED");

            return Results<Unit>.Success(Unit.Value);
        }
    }
}
