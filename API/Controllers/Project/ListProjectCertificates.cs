using Application.Interfaces;
using Application.Projects;
using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.ProjectCertificates
{
    public class ListProjectCertificates
    {
        public class Query : IRequest<IQueryable<ProjectCertificateRecord>>
        {
            public ODataQueryOptions<ProjectCertificateRecord> Options { get; set; }
            public string Language { get; set; }
        }

        public class Handler : IRequestHandler<Query, IQueryable<ProjectCertificateRecord>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<IQueryable<ProjectCertificateRecord>> Handle(Query request,
                CancellationToken cancellationToken)
            {
                var language = request.Language;

                var query = from we in _context.WorkEfforts.AsNoTracking()
                    join p in _context.Parties on we.PartyId equals p.PartyId into partyGroup
                    from p in partyGroup.DefaultIfEmpty()
                    join si in _context.StatusItems on we.CurrentStatusId equals si.StatusId into statusGroup
                    from si in statusGroup.DefaultIfEmpty()
                    where we.WorkEffortTypeId == "PROJECT_CERTIFICATE"
                    select new ProjectCertificateRecord
                    {
                        WorkEffortId = we.WorkEffortId,
                        ProjectNum = we.ProjectNum,
                        ProjectName = we.ProjectName,
                        PartyId = we.PartyId,
                        PartyName = p.Description,
                        Description = we.Description,
                        EstimatedStartDate = we.EstimatedStartDate,
                        EstimatedCompletionDate = we.EstimatedCompletionDate,
                        StatusDescription = language == "ar" ? si.DescriptionArabic : si.Description
                    };

                return query;
            }
        }
    }
}