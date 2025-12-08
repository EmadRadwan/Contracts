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
        public string? ProductName { get; set; }
        public decimal TotalPrice { get; set; } // Full amount without achievement % (i.e. 100%)
        public decimal Deserved { get; set; }
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
                    // CERTIFICATE_ITEM + Product join
                    _context.WorkEfforts
                        .Where(i => i.WorkEffortTypeId == "CERTIFICATE_ITEM")
                        .GroupJoin(
                            _context.Products,
                            item => item.ProductId,
                            prod => prod.ProductId,
                            (item, prodGroup) => new { item, prodGroup })
                        .SelectMany(
                            x => x.prodGroup.DefaultIfEmpty(),
                            (x, prod) => new
                            {
                                ParentId = x.item.WorkEffortParentId,
                                ItemId = x.item.WorkEffortId,
                                x.item.ProductId,
                                x.item.Description,
                                x.item.Quantity,
                                x.item.MaterialPrice,
                                x.item.LaborPrice,
                                x.item.AchievementPercent,
                                x.item.Deductions,
                                x.item.Insurance,
                                x.item.AdditionalInsurance,
                                // LEGACY FIELDS FOR NON-WORKMANSHIP CERTIFICATES
                                Rate = x.item.Rate,
                                TransportationExpenses = x.item.TransportationExpenses,
                                Gratuities = x.item.Gratuities,
                                Discount = x.item.Discount,
                                // Product name
                                ProductName = prod != null ? prod.ProductName : (string?)null
                            }),
                    cert => cert.we.WorkEffortId,
                    item => item.ParentId,
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

                        // Flattened item fields
                        ItemId = item.ItemId,
                        ProductId = item.ProductId,
                        Description = item.Description,
                        Quantity = item.Quantity,
                        MaterialPrice = item.MaterialPrice,
                        LaborPrice = item.LaborPrice,
                        AchievementPercent = item.AchievementPercent,
                        Deductions = item.Deductions,
                        Insurance = item.Insurance,
                        AdditionalInsurance = item.AdditionalInsurance,

                        // Legacy fields (used by Suply/Procurement certificates)
                        Rate = item.Rate,
                        TransportationExpenses = item.TransportationExpenses,
                        Gratuities = item.Gratuities,
                        Discount = item.Discount,

                        ProductName = item.ProductName
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
                        return g.Where(x => x.ItemId != null).Select(i => new ProjectCertificateSummaryDto
                        {
                            WorkEffortId = i.ItemId!, // ← from item
                            CertificateNumber = key.CertificateNumber ?? "",
                            ProjectId = key.ProjectId ?? "",
                            ProjectName = key.ProjectName,
                            PartyIdSupplier = key.PartyIdSupplier,
                            PartyNameSupplier = key.SupplierName,
                            PartyIdContractor = key.PartyIdContractor,
                            PartyNameContractor = key.ContractorName,
                            Description = i.Description ?? key.Description ?? "[Item]",
                            EstimatedStartDate = key.EstimatedStartDate,
                            EstimatedCompletionDate = key.EstimatedCompletionDate,
                            StatusDescription = key.StatusDescription,
                            StatusDescriptionArabic = key.StatusDescriptionArabic,
                            CurrentStatusId = key.CurrentStatusId ?? "",
                            CertificateCategory = key.CertificateCategory ?? "",
                            CertificateCategoryDescription = GetCertificateCategoryDescription(key.CertificateCategory),
                            FacilityId = key.FacilityId,
                            FacilityName = key.FacilityName,

                            AchievementPercent = i.AchievementPercent,

                            // ← These now come directly from the flattened item (no .Item. needed)
                            ProductName = i.ProductName ?? "—",

                            TotalPrice = Math.Round(
                                (i.Quantity ?? 0m) * ((i.MaterialPrice ?? 0m) + (i.LaborPrice ?? 0m)),
                                2),

                            Deserved = Math.Round(
                                (i.Quantity ?? 0m) * ((i.MaterialPrice ?? 0m) + (i.LaborPrice ?? 0m)) *
                                ((decimal)(i.AchievementPercent ?? 0m) / 100m),
                                2),

                            Total = Math.Round(
                                Math.Max(0m,
                                    (i.Quantity ?? 0m) *
                                    ((i.MaterialPrice ?? 0m) + (i.LaborPrice ?? 0m)) *
                                    ((decimal)(i.AchievementPercent ?? 0m) / 100m)
                                )
                                - (i.Deductions ?? 0m)
                                - (i.Insurance ?? 0m)
                                - (i.AdditionalInsurance ?? 0m),
                                2)
                        });
                    }
                    else
                    {
                        // All other types: one row per certificate (legacy formula)
                        var total = Math.Round(
                            g.Sum(i => i.ItemId == null
                                ? 0m
                                : ((i.Quantity ?? 0m) * (i.Rate ?? 0m))
                                  + (i.TransportationExpenses ?? 0m)
                                  + (i.Gratuities ?? 0m)
                                  - (i.Discount ?? 0m)),
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