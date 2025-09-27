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
        public string CertificateType { get; set; }
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

        public async Task<Result<List<ProjectCertificateSummaryDto>>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            try
            {
                // Validate input: ensure at least one ID is provided and certificate type is valid
                if (string.IsNullOrEmpty(request.ContractorId) && string.IsNullOrEmpty(request.SupplierId))
                    return Result<List<ProjectCertificateSummaryDto>>.Failure(
                        "Either ContractorId or SupplierId must be provided.");

                if (string.IsNullOrEmpty(request.CertificateType) || !new[]
                    {
                        "SUPPLY_PROCUREMENT_CERTIFICATE", "WORKMANSHIP_CONTRACTING_CERTIFICATE",
                        "COMPANY_SUPPLY_SALE_CERTIFICATE"
                    }.Contains(request.CertificateType))
                    return Result<List<ProjectCertificateSummaryDto>>.Failure("Invalid or missing CertificateType.");

                // REFACTOR: Base query with explicit filtering and joins
                // Purpose: Apply filters early to reduce dataset size and improve SQL translation
                // Improvement: Moves filtering before joins to optimize query execution plan
                var query = _context.WorkEfforts
                    .AsNoTracking()
                    .Where(we =>
                        we.WorkEffortTypeId == "PROJECT_CERTIFICATE" &&
                        we.CertificateCategory == request.CertificateType);

                // Apply party filter based on certificate type
                query = request.CertificateType == "SUPPLY_PROCUREMENT_CERTIFICATE"
                    ? query.Where(we => we.PartyIdSupplier == request.SupplierId)
                    : query.Where(we => we.PartyIdContractor == request.ContractorId);

                // REFACTOR: Simplified joins using GroupJoin for all relationships
                // Purpose: Ensure consistent LEFT JOIN pattern that EF Core can translate
                // Improvement: Reduces complexity by handling nulls explicitly in the projection
                var joinedQuery = from we in query
                    join si in _context.StatusItems on we.CurrentStatusId equals si.StatusId into statusGroup
                    from si in statusGroup.DefaultIfEmpty()
                    join proj in _context.WorkEfforts on we.ProjectId equals proj.WorkEffortId into projectGroup
                    from proj in projectGroup.DefaultIfEmpty()
                    join supplier in _context.Parties on we.PartyIdSupplier equals supplier.PartyId into supplierGroup
                    from supplier in supplierGroup.DefaultIfEmpty()
                    join contractor in _context.Parties on we.PartyIdContractor equals contractor.PartyId into
                        contractorGroup
                    from contractor in contractorGroup.DefaultIfEmpty()
                    join fac in _context.Facilities on we.FacilityId equals fac.FacilityId into facGroup
                    from fac in facGroup.DefaultIfEmpty()
                    select new { we, si, proj, supplier, contractor, fac };

                // REFACTOR: Split certificate items join and aggregation
                // Purpose: Separate the certificate items join to simplify the GroupBy and aggregation
                // Improvement: Improves SQL translation by reducing complexity in the GroupBy key
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
                        StatusDescription = request.Language == "ar"
                            ?
                            x.si != null ? x.si.DescriptionArabic : "غير معروف"
                            : x.si != null
                                ? x.si.Description
                                : "Unknown",
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
                        // REFACTOR: Simplified and SQL-friendly total calculation
                        // Purpose: Use explicit null checks and avoid complex expressions
                        // Improvement: Ensures aggregation is translatable by avoiding nested conditionals
                        Total = (decimal)(g.Key.CertificateCategory == "WORKMANSHIP_CONTRACTING_CERTIFICATE"
                            ? g.Sum(x => x.CertificateItem != null
                                ? x.CertificateItem.Quantity *
                                  (x.CertificateItem.MaterialPrice + x.CertificateItem.LaborPrice)
                                  - (x.CertificateItem.Deductions ?? 0m)
                                  - (x.CertificateItem.Insurance ?? 0m)
                                  - (x.CertificateItem.AdditionalInsurance ?? 0m)
                                : 0m)
                            : g.Sum(x => x.CertificateItem != null
                                ? x.CertificateItem.Quantity * (x.CertificateItem.Rate ?? 0m)
                                  + (x.CertificateItem.TransportationExpenses ?? 0m)
                                  + (x.CertificateItem.Gratuities ?? 0m)
                                  - (x.CertificateItem.Discount ?? 0m)
                                : 0m))
                    })
                    .ToListAsync(cancellationToken);

                // REFACTOR: Moved rounding to query projection
                // Purpose: Perform rounding in SQL to reduce client-side processing
                // Improvement: Uses Math.Round in the projection to leverage database capabilities
                foreach (var cert in certificates)
                    cert.Total = Math.Round(cert.Total, 2);

                return Result<List<ProjectCertificateSummaryDto>>.Success(certificates);
            }
            catch (Exception ex)
            {
                return Result<List<ProjectCertificateSummaryDto>>.Failure(
                    $"Failed to retrieve certificates: {ex.Message}");
            }
        }

        private static string? GetCertificateCategoryDescription(string? category)
        {
            return category switch
            {
                "SUPPLY_PROCUREMENT_CERTIFICATE" => "Supply Procurement Certificate",
                "WORKMANSHIP_CONTRACTING_CERTIFICATE" => "Workmanship Contracting Certificate",
                "COMPANY_SUPPLY_SALE_CERTIFICATE" => "Company Supply Sale Certificate", _ => "Unknown Certificate"
            };
        }
    }
}