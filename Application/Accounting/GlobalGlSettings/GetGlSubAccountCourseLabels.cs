using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.GlobalGlSettings;

/// <summary>
/// Lookup for the sixth (most granular) reporting level — SUBACCOUNT.
/// Mirrors <see cref="GetGlAccountCourseLabels"/>, with one addition: an optional
/// <see cref="GlSubAccountCourseLabelParams.GlAccountCourseLabelId"/> that narrows the list to the
/// sub-account labels actually in use under that parent account label.
///
/// WHY THE FILTER IS DATA-DRIVEN RATHER THAN SCHEMA-DRIVEN
///   gl_sub_account_course_label has no foreign key to gl_account_course_label — the two levels are
///   related only through the accounts that use them together, and the same sub-account label can
///   legitimately sit under more than one account label (4 of the 89 labels in use do). So the valid
///   pairs are derived from gl_account itself. When the requested parent has no observed pairings yet
///   the full list is returned instead, so a genuinely new combination is never a dead end.
/// </summary>
public class GetGlSubAccountCourseLabels
{
    public class GlSubAccountCourseLabelsEnvelope
    {
        public List<GlSubAccountCourseLabelDto> GlSubAccountCourseLabels { get; set; } = new();
        public int TotalCount { get; set; }

        /// <summary>
        /// True when the list was narrowed to the labels observed under the requested parent account
        /// label; false when it is the unfiltered list (no parent supplied, or no pairings yet).
        /// Lets the UI tell the user whether it is showing a suggestion or the full catalogue.
        /// </summary>
        public bool FilteredByAccountLabel { get; set; }
    }

    public class GlSubAccountCourseLabelDto
    {
        public string GlSubAccountCourseLabelId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class Query : IRequest<Result<GlSubAccountCourseLabelsEnvelope>>
    {
        public GlSubAccountCourseLabelParams? Params { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<GlSubAccountCourseLabelsEnvelope>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<GlSubAccountCourseLabelsEnvelope>> Handle(Query request, CancellationToken ct)
        {
            var accountLabelId = request.Params?.GlAccountCourseLabelId?.Trim();
            var filtered = false;

            // GlSubAccountCourseLabel has no DbSet on DataContext; Set<T>() is the pattern already used
            // elsewhere in this codebase for such entities.
            var labels = _context.Set<GlSubAccountCourseLabel>().AsQueryable();

            if (!string.IsNullOrWhiteSpace(accountLabelId))
            {
                var inUseUnderParent = _context.GlAccounts
                    .Where(a => a.GlAccountCourseLabelId == accountLabelId
                                && a.GlSubAccountCourseLabelId != null)
                    .Select(a => a.GlSubAccountCourseLabelId!)
                    .Distinct();

                // Only narrow if that parent actually has pairings — otherwise fall through to the
                // full catalogue so a new account/sub-account combination stays possible.
                if (await inUseUnderParent.AnyAsync(ct))
                {
                    labels = labels.Where(x => inUseUnderParent.Contains(x.GlSubAccountCourseLabelId));
                    filtered = true;
                }
            }

            var query = labels
                .Select(x => new GlSubAccountCourseLabelDto
                {
                    GlSubAccountCourseLabelId = x.GlSubAccountCourseLabelId,
                    Description = x.DescriptionArabic ?? x.Description ?? x.GlSubAccountCourseLabelId
                })
                .OrderBy(x => x.Description)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Params?.SearchTerm))
            {
                var term = request.Params.SearchTerm.Trim().ToLower();
                query = query.Where(x => x.Description.ToLower().Contains(term));
            }

            var total = await query.CountAsync(ct);

            var items = await query
                .Skip(request.Params?.Skip ?? 0)
                .Take(request.Params?.PageSize ?? 100)
                .ToListAsync(ct);

            var envelope = new GlSubAccountCourseLabelsEnvelope
            {
                GlSubAccountCourseLabels = items,
                TotalCount = total,
                FilteredByAccountLabel = filtered
            };

            return Result<GlSubAccountCourseLabelsEnvelope>.Success(envelope);
        }
    }
}

public class GlSubAccountCourseLabelParams
{
    public int Skip { get; set; } = 0;
    public int PageSize { get; set; } = 100;
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Optional. When supplied, narrows the result to sub-account labels already used under this
    /// account (level 5) label.
    /// </summary>
    public string? GlAccountCourseLabelId { get; set; }
}
