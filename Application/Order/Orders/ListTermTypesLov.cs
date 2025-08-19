using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;


namespace Application.Order.Orders
{
    public class ListTermTypesLov
    {
        public class Query : IRequest<Result<List<TermTypeDto>>>
        {
            public string Language { get; set; }
        }

        private static List<TermTypeDto> GetChildTermTypes(string parentId, List<TermType> termTypes, string language,
            HashSet<string> visited = null, int depth = 0, int maxDepth = 10)
        {
            // Initialize the visited set if it's null
            visited ??= new HashSet<string>();

            // Stop recursion if the maximum depth is reached or if we've already visited this node
            if (depth > maxDepth || !visited.Add(parentId))
            {
                return new List<TermTypeDto>();
            }
            return termTypes
                .Where(t => t.ParentTypeId == parentId)
                .Select(t => new TermTypeDto
                {
                    TermTypeId = t.TermTypeId,
                    Text = language == "ar" ? t.DescriptionArabic : language == "tr" ? t.DescriptionTurkish : t.Description,
                    Items = GetChildTermTypes(t.TermTypeId, termTypes, language, visited, depth + 1, maxDepth)
                })
                .ToList();
        }

        public class Handler : IRequestHandler<Query, Result<List<TermTypeDto>>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<List<TermTypeDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var language = request.Language;
                var allTermTypes = await _context.TermTypes.ToListAsync(cancellationToken);

                if (allTermTypes == null || !allTermTypes.Any())
                {
                    return Result<List<TermTypeDto>>.Failure("No Term Types Found.");
                }

                var result = allTermTypes
                    .Where(t => t.ParentTypeId == null)
                    .Select(t => new TermTypeDto
                    {
                        TermTypeId = t.TermTypeId,
                        Text = language == "ar" ? t.DescriptionArabic : language == "tr" ? t.DescriptionTurkish : t.Description,
                        Items = GetChildTermTypes(t.TermTypeId, allTermTypes, request.Language)
                    })
                    .ToList();

                return Result<List<TermTypeDto>>.Success(result);
            }
        }
    }
}