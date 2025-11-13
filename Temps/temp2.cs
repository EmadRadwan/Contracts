using Application.Interfaces;
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
        }

        public class Handler : IRequestHandler<Query, IQueryable<ProjectCertificateRecord>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context) => _context = context;

            // REFACTOR: Category description mapping (unchanged)
            private static string? GetCertificateCategoryDescription(string? category) =>
                category switch
                {
                    "SUPPLY_PROCUREMENT_CERTIFICATE" => "Supply Procurement Certificate",
                    "WORKMANSHIP_CONTRACTING_CERTIFICATE" => "Workmanship Contracting Certificate",
                    "COMPANY_SUPPLY_SALE_CERTIFICATE" => "Company Supply Sale Certificate",
                    _ => "Unknown Certificate"
                };

            public async Task<IQueryable<ProjectCertificateRecord>> Handle(Query request, CancellationToken cancellationToken)
            {
                var language = request.Language;

                // REFACTOR: Simplified TotalAmount inline — no InsuranceMode/AdditionalInsuranceMode in DB
                // Purpose: Align with frontend logic using only available DB fields
                // Context: (qty × (mat + lab) × ach%) − deductions − discount − insurance − additionalInsurance
                var itemsTotalSubquery =
                    from item in _context.WorkEfforts.AsNoTracking()
                    where item.WorkEffortTypeId == "CERTIFICATE_ITEM"
                    join parent in _context.WorkEfforts.AsNoTracking()
                        on item.WorkEffortParentId equals parent.WorkEffortId
                    where parent.WorkEffortTypeId == "PROJECT_CERTIFICATE"
                    let isWorkmanship = parent.CertificateCategory == "WORKMANSHIP_CONTRACTING_CERTIFICATE"
                    let baseAmount = (item.Quantity ?? 0m) * ((item.MaterialPrice ?? 0m) + (item.LaborPrice ?? 0m))
                    let achievedAmount = baseAmount * ((item.AchievementPercent ?? 100m) / 100m)
                    let afterDeductions = achievedAmount - (item.Deductions ?? 0m) - (item.Discount ?? 0m)
                    let netAfterInsurance = afterDeductions - (item.Insurance ?? 0m)
                    let finalItemTotal = netAfterInsurance - (item.AdditionalInsurance ?? 0m)
                    let otherTotal = item.TotalAmount ??
                                     ((item.Quantity ?? 0m) * (item.Rate ?? 0m)) +
                                     (item.TransportationExpenses ?? 0m) +
                                     (item.Gratuities ?? 0m) -
                                     (item.Discount ?? 0m)
                    select new
                    {
                        WorkEffortParentId = item.WorkEffortParentId,
                        ItemTotal = isWorkmanship ? finalItemTotal : otherTotal
                    } into grouped
                    group grouped by grouped.WorkEffortParentId into g
                    select new
                    {
                        WorkEffortParentId = g.Key,
                        TotalAmount = g.Sum(x => x.ItemTotal)
                    };

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
                    join total in itemsTotalSubquery on we.WorkEffortId equals total.WorkEffortParentId into totalGroup
                    from total in totalGroup.DefaultIfEmpty()
                    where we.WorkEffortTypeId == "PROJECT_CERTIFICATE"
                    select new ProjectCertificateRecord
                    {
                        WorkEffortId = we.WorkEffortId,
                        CertificateNumber = we.CertificateNumber,
                        CertificateCategory = we.CertificateCategory,
                        CertificateCategoryDescription = null,
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
                        TotalAmount = total != null ? total.TotalAmount : 0m
                    };

                var result = query.Select(record => new ProjectCertificateRecord
                {
                    WorkEffortId = record.WorkEffortId,
                    CertificateNumber = record.CertificateNumber,
                    CertificateCategory = record.CertificateCategory,
                    CertificateCategoryDescription = GetCertificateCategoryDescription(record.CertificateCategory),
                    ProjectId = record.ProjectId,
                    ProjectName = record.ProjectName,
                    Description = record.Description,
                    EstimatedStartDate = record.EstimatedStartDate,
                    EstimatedCompletionDate = record.EstimatedCompletionDate,
                    StatusDescription = record.StatusDescription,
                    CurrentStatusId = record.CurrentStatusId,
                    PartyIdSupplier = record.PartyIdSupplier,
                    PartyNameSupplier = record.PartyNameSupplier,
                    PartyIdContractor = record.PartyIdContractor,
                    PartyNameContractor = record.PartyNameContractor,
                    RelatedOrderId = record.RelatedOrderId,
                    FacilityId = record.FacilityId,
                    FacilityName = record.FacilityName,
                    TotalAmount = record.TotalAmount
                });

                return result;
            }
        }
    }
}