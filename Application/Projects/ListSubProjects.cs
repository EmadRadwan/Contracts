using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Projects;

public class ListSubProjects
{
    // REFACTOR: Define SubProjectDto to match frontend SubProject interface, ensuring only necessary fields are returned
    public class SubProjectDto
    {
        public string WorkEffortId { get; set; }
        public string SubProjectName { get; set; }
        public string ProjectId { get; set; }
    }

    public class Query : IRequest<Result<List<SubProjectDto>>>
    {
        public string ProjectId { get; set; }
        public string Language { get; set; }
    }

    // REFACTOR: Add validator to ensure ProjectId and Language are provided, consistent with ListCertificateItems
    public class QueryValidator : AbstractValidator<Query>
    {
        public QueryValidator()
        {
            RuleFor(x => x.ProjectId).NotEmpty().WithMessage("Project ID is required");
            RuleFor(x => x.Language).NotEmpty().WithMessage("Language is required");
        }
    }

    public class Handler : IRequestHandler<Query, Result<List<SubProjectDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<SubProjectDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            // REFACTOR: Validate input parameters, aligning with ListCertificateItems validation pattern
            var validator = new QueryValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
                return Result<List<SubProjectDto>>.Failure(string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));

            var language = request.Language?.ToLower() == "en" ? "en" : "ar";

            try
            {
                // REFACTOR: Query sub-projects by ProjectId and WORK_EFFORT_TYPE_ID, selecting only required fields
                var subProjects = await _context.WorkEfforts
                    .Where(we => we.ProjectId == request.ProjectId && we.WorkEffortTypeId == "SUB_PROJECT")
                    .Select(we => new SubProjectDto
                    {
                        WorkEffortId = we.WorkEffortId,
                        SubProjectName = we.SubProjectName,
                        ProjectId = we.ProjectId
                    })
                    .ToListAsync(cancellationToken);

                return Result<List<SubProjectDto>>.Success(subProjects);
            }
            catch (Exception ex)
            {
                // REFACTOR: Handle errors consistently with ListCertificateItems, providing clear error messages
                return Result<List<SubProjectDto>>.Failure($"Failed to retrieve sub-projects: {ex.Message}");
            }
        }
    }
}