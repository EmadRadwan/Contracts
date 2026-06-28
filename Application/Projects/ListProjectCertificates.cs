using Application.Projects;
using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Projects
{
    public class ListProjectCertificates
    {
        public class Query : IRequest<IQueryable<ProjectCertificateRecord>>
        {
            public ODataQueryOptions<ProjectCertificateRecord> Options { get; set; }
            public string Language { get; set; }
            public DateOnly? FromDate { get; set; }
            public DateOnly? ToDate { get; set; }
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

                var query =
                    from we in _context.WorkEfforts.AsNoTracking()
                    join si in _context.StatusItems on we.CurrentStatusId equals si.StatusId into statusGroup
                    from si in statusGroup.DefaultIfEmpty()
                    join proj in _context.WorkEfforts on we.ProjectId equals proj.WorkEffortId into projectGroup
                    from proj in projectGroup.DefaultIfEmpty()
                    join supplier in _context.Parties on we.PartyIdSupplier equals supplier.PartyId into supplierGroup
                    from supplier in supplierGroup.DefaultIfEmpty()
                    join contractor in _context.Parties on we.PartyIdContractor equals contractor.PartyId into contractorGroup
                    from contractor in contractorGroup.DefaultIfEmpty()
                    join fac in _context.Facilities on we.FacilityId equals fac.FacilityId into facGroup
                    from fac in facGroup.DefaultIfEmpty()
                    where we.WorkEffortTypeId == "PROJECT_CERTIFICATE"
                    select new ProjectCertificateRecord
                    {
                        WorkEffortId = we.WorkEffortId,
                        CertificateNumber = we.CertificateNumber,
                        CertificateCategory = we.CertificateCategory,
                        CertificateCategoryDescription =
                            we.CertificateCategory == "SUPPLY_PROCUREMENT_CERTIFICATE" ? "Supply Procurement Certificate" :
                            we.CertificateCategory == "WORKMANSHIP_CONTRACTING_CERTIFICATE" ? "Workmanship Contracting Certificate" :
                            we.CertificateCategory == "COMPANY_SUPPLY_SALE_CERTIFICATE" ? "Company Supply Sale Certificate" :
                            "Unknown Certificate",
                        ProjectId = we.ProjectId,
                        ProjectName = proj != null ? proj.ProjectName : we.ProjectName,
                        Description = we.Description,
                        EstimatedStartDate = we.EstimatedStartDate,
                        EstimatedCompletionDate = we.EstimatedCompletionDate,
                        StatusDescription = language == "ar" ? si.DescriptionArabic : si.Description,
                        CurrentStatusId = we.CurrentStatusId,
                        PartyIdSupplier = supplier != null ? supplier.PartyId : null,
                        PartyNameSupplier = supplier != null ? supplier.Description : null,
                        PartyIdContractor = contractor != null ? contractor.PartyId : null,
                        PartyNameContractor = contractor != null ? contractor.Description : null,
                        RelatedOrderId = we.RelatedOrderId,
                        FacilityId = we.FacilityId,
                        FacilityName = fac != null ? fac.FacilityName : null,
                        // Correlated subquery avoids GroupBy translation issues in EF Core.
                        // we.CertificateCategory is the parent certificate type — same as the old
                        // parent.CertificateCategory check in the former itemsTotalSubquery.
                        TotalAmount = _context.WorkEfforts.AsNoTracking()
                            .Where(i => i.WorkEffortTypeId == "CERTIFICATE_ITEM"
                                        && i.WorkEffortParentId == we.WorkEffortId)
                            .Sum(i => we.CertificateCategory == "WORKMANSHIP_CONTRACTING_CERTIFICATE"
                                ? (i.Quantity ?? 0m) * ((i.MaterialPrice ?? 0m) + (i.LaborPrice ?? 0m))
                                  * ((i.AchievementPercent ?? 100m) / 100m)
                                  - (i.Deductions ?? 0m) - (i.Discount ?? 0m)
                                  - (i.Insurance ?? 0m) - (i.AdditionalInsurance ?? 0m)
                                : i.TotalAmount ??
                                  (i.Quantity ?? 0m) * (i.Rate ?? 0m)
                                  + (i.TransportationExpenses ?? 0m)
                                  + (i.Gratuities ?? 0m)
                                  - (i.Discount ?? 0m)
                            ),
                        CreatedDate = we.CreatedDate
                    };

                if (request.FromDate.HasValue)
                {
                    var from = request.FromDate.Value.ToDateTime(TimeOnly.MinValue);
                    query = query.Where(we => we.EstimatedStartDate >= from);
                }
                if (request.ToDate.HasValue)
                {
                    var to = request.ToDate.Value.ToDateTime(TimeOnly.MaxValue);
                    query = query.Where(we => we.EstimatedStartDate <= to);
                }

                return query;
            }
        }
    }
}
