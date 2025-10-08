using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Projects;

public class GetCertificatesByParty
{
    public class Query : IRequest<Result<List<ProjectCertificateSummaryDto>>>
    {
        public string? ContractorId { get; set; }
        public string? SupplierId { get; set; }
        public string Language { get; set; }
    }

    public class ProjectCertificateSummaryDto
    {
        public string WorkEffortId { get; set; }
        public string CertificateNumber { get; set; }
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string PartyIdSupplier { get; set; }
        public string PartyNameSupplier { get; set; }
        public string PartyIdContractor { get; set; }
        public string PartyNameContractor { get; set; }
        public string Description { get; set; }
        public DateTime? EstimatedStartDate { get; set; }
        public DateTime? EstimatedCompletionDate { get; set; }
        public string StatusDescription { get; set; }
        public string StatusDescriptionArabic { get; set; }
        public string CurrentStatusId { get; set; }
        public string CertificateCategory { get; set; }
        public string CertificateCategoryDescription { get; set; }
        public string FacilityId { get; set; }
        public string FacilityName { get; set; }
        public decimal Total { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<List<ProjectCertificateSummaryDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<ProjectCertificateSummaryDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            try
            {
                // Validate input: ensure at least one ID is provided
                if (string.IsNullOrEmpty(request.ContractorId) && string.IsNullOrEmpty(request.SupplierId))
                    return Result<List<ProjectCertificateSummaryDto>>.Failure(
                        "Either ContractorId or SupplierId must be provided.");

                // REFACTOR: Log input parameters for debugging
                // Purpose: Capture input values to verify query parameters
                // Improvement: Helps confirm ContractorId and SupplierId values
                Console.WriteLine($"Input: ContractorId={request.ContractorId}, SupplierId={request.SupplierId}, Language={request.Language}");

                // REFACTOR: Simplified base query without CertificateType filter
                // Purpose: Remove CertificateType condition to include all PROJECT_CERTIFICATE records
                // Improvement: Ensures all records for the contractor are returned regardless of category
                var query = _context.WorkEfforts
                    .AsNoTracking()
                    .Where(we => we.WorkEffortTypeId == "PROJECT_CERTIFICATE");

                // REFACTOR: Flexible party filter for ContractorId or SupplierId
                // Purpose: Include records matching either ContractorId or SupplierId
                // Improvement: Prevents exclusion of records based on party type
                query = query.Where(we => 
                    (request.ContractorId != null && we.PartyIdContractor == request.ContractorId) ||
                    (request.SupplierId != null && we.PartyIdSupplier == request.SupplierId));

                // REFACTOR: Log filtered WorkEffort IDs before joins
                // Purpose: Verify which records pass the initial filters
                // Improvement: Identifies if records are filtered out early
                var filteredIds = await query.Select(we => we.WorkEffortId).ToListAsync(cancellationToken);
                Console.WriteLine($"Filtered WorkEffort IDs: {string.Join(", ", filteredIds)}");

                // REFACTOR: Simplified joins with explicit null handling
                // Purpose: Ensure LEFT JOINs don't filter out records
                // Improvement: Reduces risk of missing records due to join failures
                var joinedQuery = from we in query
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
                                 select new { we, si, proj, supplier, contractor, fac };

                // REFACTOR: Log joined WorkEffort IDs
                // Purpose: Confirm which records survive the joins
                // Improvement: Identifies if joins are filtering out records
                var joinedIds = await joinedQuery.Select(x => x.we.WorkEffortId).ToListAsync(cancellationToken);
                Console.WriteLine($"Joined WorkEffort IDs: {string.Join(", ", joinedIds)}");

                // REFACTOR: Simplified certificate items join with null handling
                // Purpose: Ensure CERTIFICATE_ITEM join doesn't exclude records
                // Improvement: Uses DefaultIfEmpty to include certificates without items
                var certificates = await joinedQuery
                    .GroupJoin(
                        _context.WorkEfforts.AsNoTracking().Where(we => we.WorkEffortTypeId == "CERTIFICATE_ITEM"),
                        cert => cert.we.WorkEffortId,
                        item => item.WorkEffortParentId,
                        (cert, items) => new { cert, items }
                    )
                    .SelectMany(
                        x => x.items.DefaultIfEmpty(),
                        (cert, item) => new
                        {
                            cert.cert.we,
                            cert.cert.si,
                            cert.cert.proj,
                            cert.cert.supplier,
                            cert.cert.contractor,
                            cert.cert.fac,
                            CertificateItem = item
                        }
                    )
                    .GroupBy(x => new
                    {
                        x.we.WorkEffortId,
                        x.we.CertificateNumber,
                        x.we.CertificateCategory,
                        x.we.ProjectId,
                        ProjectName = x.proj != null ? x.proj.ProjectName : x.we.ProjectName,
                        x.we.Description,
                        x.we.EstimatedStartDate,
                        x.we.EstimatedCompletionDate,
                        x.we.CurrentStatusId,
                        StatusDescription = request.Language == "ar" ? x.si != null ? x.si.DescriptionArabic : "غير معروف" : x.si != null ? x.si.Description : "Unknown",
                        StatusDescriptionArabic = x.si != null ? x.si.DescriptionArabic : "غير معروف",
                        x.we.PartyIdSupplier,
                        SupplierDescription = x.supplier != null ? x.supplier.Description : null,
                        x.we.PartyIdContractor,
                        ContractorDescription = x.contractor != null ? x.contractor.Description : null,
                        x.we.FacilityId,
                        FacilityName = x.fac != null ? x.fac.FacilityName : null
                    })
                    .Select(g => new ProjectCertificateSummaryDto
                    {
                        WorkEffortId = g.Key.WorkEffortId,
                        CertificateNumber = g.Key.CertificateNumber,
                        CertificateCategory = g.Key.CertificateCategory,
                        CertificateCategoryDescription = GetCertificateCategoryDescription(g.Key.CertificateCategory),
                        ProjectId = g.Key.ProjectId,
                        ProjectName = g.Key.ProjectName ?? "",
                        Description = g.Key.Description ?? "",
                        EstimatedStartDate = g.Key.EstimatedStartDate,
                        EstimatedCompletionDate = g.Key.EstimatedCompletionDate,
                        StatusDescription = g.Key.StatusDescription,
                        StatusDescriptionArabic = g.Key.StatusDescriptionArabic,
                        CurrentStatusId = g.Key.CurrentStatusId,
                        PartyIdSupplier = g.Key.PartyIdSupplier,
                        PartyNameSupplier = g.Key.SupplierDescription,
                        PartyIdContractor = g.Key.PartyIdContractor,
                        PartyNameContractor = g.Key.ContractorDescription,
                        FacilityId = g.Key.FacilityId,
                        FacilityName = g.Key.FacilityName,
                        // REFACTOR: Simplified total calculation with null-safe defaults
                        // Purpose: Ensure Total is calculated even if no CertificateItem exists
                        // Improvement: Prevents null reference issues and ensures SQL compatibility
                        Total = Math.Round((decimal)(g.Key.CertificateCategory == "WORKMANSHIP_CONTRACTING_CERTIFICATE"
                            ? g.Sum(x => x.CertificateItem != null 
                                ? (x.CertificateItem.Quantity ?? 0m) * ((x.CertificateItem.MaterialPrice ?? 0m) + (x.CertificateItem.LaborPrice ?? 0m)) 
                                  - (x.CertificateItem.Deductions ?? 0m) 
                                  - (x.CertificateItem.Insurance ?? 0m) 
                                  - (x.CertificateItem.AdditionalInsurance ?? 0m) 
                                : 0m)
                            : g.Sum(x => x.CertificateItem != null 
                                ? (x.CertificateItem.Quantity ?? 0m) * (x.CertificateItem.Rate ?? 0m) 
                                  + (x.CertificateItem.TransportationExpenses ?? 0m) 
                                  + (x.CertificateItem.Gratuities ?? 0m) 
                                  - (x.CertificateItem.Discount ?? 0m) 
                                : 0m)), 2)
                    })
                    .ToListAsync(cancellationToken);

                // REFACTOR: Log final result count and IDs
                // Purpose: Confirm how many records are returned and their IDs
                // Improvement: Helps diagnose if all expected records are included
                Console.WriteLine($"Returned Certificates Count: {certificates.Count}");
                Console.WriteLine($"Returned WorkEffort IDs: {string.Join(", ", certificates.Select(c => c.WorkEffortId))}");

                return Result<List<ProjectCertificateSummaryDto>>.Success(certificates);
            }
            catch (Exception ex)
            {
                // REFACTOR: Include stack trace in error message
                // Purpose: Provide more context for debugging
                // Improvement: Helps identify the exact point of failure
                return Result<List<ProjectCertificateSummaryDto>>.Failure(
                    $"Failed to retrieve certificates: {ex.Message}\nStack Trace: {ex.StackTrace}");
            }
        }

        private static string? GetCertificateCategoryDescription(string? category)
        {
            return category switch
            {
                "SUPPLY_PROCUREMENT_CERTIFICATE" => "Supply Procurement Certificate",
                "WORKMANSHIP_CONTRACTING_CERTIFICATE" => "Workmanship Contracting Certificate",
                "COMPANY_SUPPLY_SALE_CERTIFICATE" => "Company Supply Sale Certificate",
                _ => "Unknown Certificate"
            };
        }
    }
}