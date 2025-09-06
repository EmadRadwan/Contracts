using Application.Interfaces;
using Application.Projects;
using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Projects
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

            public async Task<IQueryable<ProjectCertificateRecord>> Handle(Query request, CancellationToken cancellationToken)
            {
                var language = request.Language;

                var query = from we in _context.WorkEfforts.AsNoTracking()
                            join p in _context.Parties on we.PartyId equals p.PartyId into partyGroup
                            from p in partyGroup.DefaultIfEmpty()
                            join si in _context.StatusItems on we.CurrentStatusId equals si.StatusId into statusGroup
                            from si in statusGroup.DefaultIfEmpty()
                            join proj in _context.WorkEfforts on we.ProjectId equals proj.WorkEffortId into projectGroup
                            from proj in projectGroup.DefaultIfEmpty()
                            where we.WorkEffortTypeId == "PROJECT_CERTIFICATE"
                            select new ProjectCertificateRecord
                            {
                                WorkEffortId = we.WorkEffortId,
                                CertificateNumber = we.CertificateNumber,
                                CertificateCategory = we.CertificateCategory,
                                // REFACTOR: Added CertificateCategoryDescription to map CertificateCategory values
                                // to human-readable text. CONTRACTING_CERTIFICATE maps to "Contracting Certificate"
                                // and PROCUREMENT_CERTIFICATE maps to "Procurement Certificate". Uses a ternary
                                // operator for concise mapping, with a fallback for unexpected values.
                                CertificateCategoryDescription = we.CertificateCategory == "CONTRACTING_CERTIFICATE" ? "Contracting Certificate" :
                                                                we.CertificateCategory == "PROCUREMENT_CERTIFICATE" ? "Procurement Certificate" :
                                                                "Unknown Certificate",
                                ProjectName = proj != null ? proj.ProjectName : we.ProjectName,
                                ProjectId = we.ProjectId,
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