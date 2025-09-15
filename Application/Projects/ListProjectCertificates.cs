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

            public Handler(DataContext context)
            {
                _context = context;
            }

            // REFACTOR: Moved CertificateCategoryDescription mapping to a separate method
            // Purpose: Avoids using switch expression in LINQ query to prevent CS8514 error
            // Context: EF Core cannot translate switch expressions; mapping is done in memory
            private static string? GetCertificateCategoryDescription(string? category)
            {
                return category switch
                {
                    "SUPPLY_PROCUREMENT_CERTIFICATE" => "Supply Procurement Certificate",
                    "WORKMANSHIP_CONTRACTING_CERTIFICATE" => "Workmanship Contracting Certificate",
                    "CONTRACTOR_PURCHASE_CERTIFICATE" => "Contractor Purchase Certificate",
                    "COMPANY_SUPPLY_SALE_CERTIFICATE" => "Company Supply Sale Certificate",
                    "EXTERNAL_SUPPLY_SALE_CERTIFICATE" => "External Supply Sale Certificate",
                    _ => "Unknown Certificate"
                };
            }

            public async Task<IQueryable<ProjectCertificateRecord>> Handle(Query request, CancellationToken cancellationToken)
            {
                var language = request.Language;

                // REFACTOR: Added join to Facilities table for FacilityId and FacilityName
                // Purpose: Include facility data at header level in ProjectCertificateRecord
                // Improvement: Enables frontend to display facility without additional queries
                // Context: FacilityId moved from item to header; left join ensures null for non-facility types
                var query = from we in _context.WorkEfforts.AsNoTracking()
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
                                CertificateCategoryDescription = null, // Set to null initially; mapped in memory below
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
                                // REFACTOR: Added FacilityId and FacilityName to projection
                                // Purpose: Map facility data from joined Facilities table
                                // Improvement: Provides header-level facility info for frontend binding
                                // Context: Matches updated ProjectCertificateRecord; null for non-facility types
                                FacilityId = we.FacilityId,
                                FacilityName = fac != null ? fac.FacilityName : null
                            };

                // REFACTOR: Map CertificateCategory to CertificateCategoryDescription in memory
                // Purpose: Apply description mapping after query execution to avoid EF Core limitations
                // Context: Uses GetCertificateCategoryDescription to set the field
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
                    FacilityName = record.FacilityName
                });

                // REFACTOR: Retained debug logging for query result
                // Purpose: Verify PartyIdSupplier/PartyNameSupplier and FacilityId/FacilityName presence in response
                // Improvement: Helps diagnose binding issues in frontend
                // Context: Logs sample for debugging; includes new facility fields
                //var resultList = await result.Take(10).ToListAsync(cancellationToken);
                //Console.WriteLine("ListProjectCertificates query result sample: " + System.Text.Json.JsonSerializer.Serialize(resultList));

                return result;
            }
        }
    }
}