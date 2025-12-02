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
        public string Language { get; set; } = "en";
    }

    public class ProjectCertificateSummaryDto
    {
        public string WorkEffortId { get; set; } = default!;
        public string CertificateNumber { get; set; } = default!;
        public string ProjectId { get; set; } = default!;
        public string ProjectName { get; set; } = default!;
        public string? PartyIdSupplier { get; set; }
        public string? PartyNameSupplier { get; set; }
        public string? PartyIdContractor { get; set; }
        public string? PartyNameContractor { get; set; }
        public string? Description { get; set; }
        public DateTime? EstimatedStartDate { get; set; }
        public DateTime? EstimatedCompletionDate { get; set; }
        public string StatusDescription { get; set; } = default!;
        public string StatusDescriptionArabic { get; set; } = default!;
        public string CurrentStatusId { get; set; } = default!;
        public string CertificateCategory { get; set; } = default!;
        public string CertificateCategoryDescription { get; set; } = default!;
        public string? FacilityId { get; set; }
        public string? FacilityName { get; set; }
        public decimal Total { get; set; }
        public decimal? AchievementPercent { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<List<ProjectCertificateSummaryDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context) => _context = context;

        public async Task<Result<List<ProjectCertificateSummaryDto>>> Handle(
            Query request,
            CancellationToken ct)
        {
            // REFACTOR: Validate input
            if (string.IsNullOrWhiteSpace(request.ContractorId) &&
                string.IsNullOrWhiteSpace(request.SupplierId))
                return Result<List<ProjectCertificateSummaryDto>>.Failure(
                    "Either ContractorId or SupplierId must be provided.");

            var lang = request.Language?.ToLowerInvariant() == "ar" ? "ar" : "en";

            var baseQuery = _context.WorkEfforts
                .AsNoTracking()
                .Where(we => we.WorkEffortTypeId == "PROJECT_CERTIFICATE");

            var partyFiltered = baseQuery.Where(we =>
                (request.ContractorId != null && we.PartyIdContractor == request.ContractorId) ||
                (request.SupplierId != null && we.PartyIdSupplier == request.SupplierId));

            var joined = partyFiltered
                .GroupJoin(_context.StatusItems, we => we.CurrentStatusId, si => si.StatusId,
                    (we, siGroup) => new { we, siGroup })
                .SelectMany(x => x.siGroup.DefaultIfEmpty(), (x, si) => new { x.we, si })
                .GroupJoin(_context.WorkEfforts, x => x.we.ProjectId, proj => proj.WorkEffortId,
                    (x, projGroup) => new { x.we, x.si, projGroup })
                .SelectMany(x => x.projGroup.DefaultIfEmpty(), (x, proj) => new { x.we, x.si, proj })
                .GroupJoin(_context.Parties, x => x.we.PartyIdSupplier, p => p.PartyId,
                    (x, supGroup) => new { x.we, x.si, x.proj, supGroup })
                .SelectMany(x => x.supGroup.DefaultIfEmpty(), (x, sup) => new { x.we, x.si, x.proj, sup })
                .GroupJoin(_context.Parties, x => x.we.PartyIdContractor, p => p.PartyId,
                    (x, conGroup) => new { x.we, x.si, x.proj, x.sup, conGroup })
                .SelectMany(x => x.conGroup.DefaultIfEmpty(), (x, con) => new { x.we, x.si, x.proj, x.sup, con })
                .GroupJoin(_context.Facilities, x => x.we.FacilityId, f => f.FacilityId,
                    (x, facGroup) => new { x.we, x.si, x.proj, x.sup, x.con, facGroup })
                .SelectMany(x => x.facGroup.DefaultIfEmpty(),
                    (x, fac) => new { x.we, x.si, x.proj, x.sup, x.con, fac });

            // Join with items
            var withItemsQuery = joined
                .GroupJoin(
                    _context.WorkEfforts.Where(i => i.WorkEffortTypeId == "CERTIFICATE_ITEM"),
                    cert => cert.we.WorkEffortId,
                    item => item.WorkEffortParentId,
                    (cert, items) => new { cert.we, cert.si, cert.proj, cert.sup, cert.con, cert.fac, items })
                .SelectMany(
                    x => x.items.DefaultIfEmpty(),
                    (x, item) => new
                    {
                        Certificate = x.we,
                        StatusItem = x.si,
                        Project = x.proj,
                        Supplier = x.sup,
                        Contractor = x.con,
                        Facility = x.fac,
                        Item = item
                    });

            // Bring data into memory — required for complex logic
            var data = await withItemsQuery.ToListAsync(ct);

            // Final projection in memory
            var result = data
                .GroupBy(g => new
                {
                    g.Certificate.WorkEffortId,
                    g.Certificate.CertificateNumber,
                    g.Certificate.CertificateCategory,
                    g.Certificate.ProjectId,
                    ProjectName = g.Project?.ProjectName ?? g.Certificate.ProjectName ?? "",
                    g.Certificate.Description,
                    g.Certificate.EstimatedStartDate,
                    g.Certificate.EstimatedCompletionDate,
                    g.Certificate.CurrentStatusId,
                    StatusDescription = lang == "ar"
                        ? g.StatusItem?.DescriptionArabic ?? "غير معروف"
                        : g.StatusItem?.Description ?? "Unknown",
                    StatusDescriptionArabic = g.StatusItem?.DescriptionArabic ?? "غير معروف",
                    g.Certificate.PartyIdSupplier,
                    SupplierName = g.Supplier?.Description,
                    g.Certificate.PartyIdContractor,
                    ContractorName = g.Contractor?.Description,
                    g.Certificate.FacilityId,
                    FacilityName = g.Facility?.FacilityName
                })
                .SelectMany(g =>
                {
                    var key = g.Key;
                    var isWorkmanship = key.CertificateCategory == "WORKMANSHIP_CONTRACTING_CERTIFICATE";

                    // WORKMANSHIP: Return ONE ROW PER ITEM (with correct achievement-based total)
                    if (isWorkmanship)
                    {
                        return g.Where(x => x.Item != null).Select(i => new ProjectCertificateSummaryDto
                        {
                            WorkEffortId = i.Item.WorkEffortId!,
                            CertificateNumber = key.CertificateNumber ?? "",
                            ProjectId = key.ProjectId ?? "",
                            ProjectName = key.ProjectName,
                            PartyIdSupplier = key.PartyIdSupplier,
                            PartyNameSupplier = key.SupplierName,
                            PartyIdContractor = key.PartyIdContractor,
                            PartyNameContractor = key.ContractorName,
                            Description = i.Item.Description ?? key.Description ?? "[Item]",
                            EstimatedStartDate = key.EstimatedStartDate,
                            EstimatedCompletionDate = key.EstimatedCompletionDate,
                            StatusDescription = key.StatusDescription,
                            StatusDescriptionArabic = key.StatusDescriptionArabic,
                            CurrentStatusId = key.CurrentStatusId ?? "",
                            CertificateCategory = key.CertificateCategory ?? "",
                            CertificateCategoryDescription = GetCertificateCategoryDescription(key.CertificateCategory),
                            FacilityId = key.FacilityId,
                            FacilityName = key.FacilityName,
                            AchievementPercent = i.Item.AchievementPercent,
                            Total = Math.Round(
                                Math.Max(0m,
                                    (i.Item.Quantity ?? 0m) *
                                    ((i.Item.MaterialPrice ?? 0m) + (i.Item.LaborPrice ?? 0m)) *
                                    ((decimal)(i.Item.AchievementPercent ?? 0m) / 100m)
                                )
                                - (i.Item.Deductions ?? 0m)
                                - (i.Item.Insurance ?? 0m)
                                - (i.Item.AdditionalInsurance ?? 0m),
                                2)
                        });
                    }
                    else
                    {
                        // All other types: one row per certificate (legacy formula)
                        var total = Math.Round(
                            g.Sum(i => i.Item == null
                                ? 0m
                                : ((i.Item.Quantity ?? 0m) * (i.Item.Rate ?? 0m)) +
                                  (i.Item.TransportationExpenses ?? 0m) +
                                  (i.Item.Gratuities ?? 0m) -
                                  (i.Item.Discount ?? 0m)),
                            2);

                        return new[]
                        {
                            new ProjectCertificateSummaryDto
                            {
                                WorkEffortId = key.WorkEffortId,
                                CertificateNumber = key.CertificateNumber ?? "",
                                ProjectId = key.ProjectId ?? "",
                                ProjectName = key.ProjectName,
                                PartyIdSupplier = key.PartyIdSupplier,
                                PartyNameSupplier = key.SupplierName,
                                PartyIdContractor = key.PartyIdContractor,
                                PartyNameContractor = key.ContractorName,
                                Description = key.Description ?? "",
                                EstimatedStartDate = key.EstimatedStartDate,
                                EstimatedCompletionDate = key.EstimatedCompletionDate,
                                StatusDescription = key.StatusDescription,
                                StatusDescriptionArabic = key.StatusDescriptionArabic,
                                CurrentStatusId = key.CurrentStatusId ?? "",
                                CertificateCategory = key.CertificateCategory ?? "",
                                CertificateCategoryDescription =
                                    GetCertificateCategoryDescription(key.CertificateCategory),
                                FacilityId = key.FacilityId,
                                FacilityName = key.FacilityName,
                                Total = total
                            }
                        };
                    }
                })
                .OrderBy(x => x.CertificateNumber)
                .ThenBy(x => x.WorkEffortId)
                .ToList();

            return Result<List<ProjectCertificateSummaryDto>>.Success(result);
        }

        private static string GetCertificateCategoryDescription(string? category) => category switch
        {
            "SUPPLY_PROCUREMENT_CERTIFICATE" => "Supply Procurement Certificate",
            "WORKMANSHIP_CONTRACTING_CERTIFICATE" => "Workmanship Contracting Certificate",
            "COMPANY_SUPPLY_SALE_CERTIFICATE" => "Company Supply Sale Certificate",
            _ => "Unknown Certificate"
        };
    }
}