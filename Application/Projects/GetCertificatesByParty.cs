using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Projects;

public class GetCertificatesByParty
{
    public class Query : IRequest<Result<List<ProjectCertificateSummaryDto>>>
    {
        public string? ContractorId { get; set; }
        public string? SupplierId   { get; set; }
        public string  Language     { get; set; } = "en";
    }

    public class ProjectCertificateSummaryDto
    {
        public string   WorkEffortId               { get; set; } = default!;
        public string   CertificateNumber          { get; set; } = default!;
        public string   ProjectId                  { get; set; } = default!;
        public string   ProjectName                { get; set; } = default!;
        public string?  PartyIdSupplier            { get; set; }
        public string?  PartyNameSupplier          { get; set; }
        public string?  PartyIdContractor          { get; set; }
        public string?  PartyNameContractor        { get; set; }
        public string?  Description                { get; set; }
        public DateTime? EstimatedStartDate        { get; set; }
        public DateTime? EstimatedCompletionDate   { get; set; }
        public string   StatusDescription          { get; set; } = default!;
        public string   StatusDescriptionArabic    { get; set; } = default!;
        public string   CurrentStatusId            { get; set; } = default!;
        public string   CertificateCategory        { get; set; } = default!;
        public string   CertificateCategoryDescription { get; set; } = default!;
        public string?  FacilityId                 { get; set; }
        public string?  FacilityName               { get; set; }
        public decimal  Total                      { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<List<ProjectCertificateSummaryDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context) => _context = context;

        public async Task<Result<List<ProjectCertificateSummaryDto>>> Handle(
            Query request,
            CancellationToken ct)
        {
            // REFACTOR: Early validation – at least one party must be supplied
            // Purpose: Guarantees the query is meaningful and avoids a full table scan
            if (string.IsNullOrWhiteSpace(request.ContractorId) &&
                string.IsNullOrWhiteSpace(request.SupplierId))
                return Result<List<ProjectCertificateSummaryDto>>.Failure(
                    "Either ContractorId or SupplierId must be provided.");

            // REFACTOR: Normalise language once (defaults to English)
            // Purpose: Avoid repeated ternary checks later
            var lang = request.Language?.ToLowerInvariant() == "ar" ? "ar" : "en";

            // REFACTOR: Base query – only PROJECT_CERTIFICATE records, no extra filters
            // Purpose: Keep the set small and let EF translate everything to SQL
            var baseQuery = _context.WorkEfforts
                .AsNoTracking()
                .Where(we => we.WorkEffortTypeId == "PROJECT_CERTIFICATE");

            // REFACTOR: Party filter – OR logic for ContractorId / SupplierId
            // Purpose: Allows the same handler to be used for either party type
            var partyFiltered = baseQuery.Where(we =>
                (request.ContractorId != null && we.PartyIdContractor == request.ContractorId) ||
                (request.SupplierId   != null && we.PartyIdSupplier   == request.SupplierId));

            // REFACTOR: All LEFT JOINs in one expression using GroupJoin + SelectMany
            // Purpose: Guarantees every certificate is returned even when related rows are missing
            var joined = partyFiltered
                .GroupJoin(_context.StatusItems,
                    we => we.CurrentStatusId,
                    si => si.StatusId,
                    (we, siGroup) => new { we, siGroup })
                .SelectMany(x => x.siGroup.DefaultIfEmpty(),
                    (x, si) => new { x.we, si })

                .GroupJoin(_context.WorkEfforts,
                    x => x.we.ProjectId,
                    proj => proj.WorkEffortId,
                    (x, projGroup) => new { x.we, x.si, projGroup })
                .SelectMany(x => x.projGroup.DefaultIfEmpty(),
                    (x, proj) => new { x.we, x.si, proj })

                .GroupJoin(_context.Parties,
                    x => x.we.PartyIdSupplier,
                    p => p.PartyId,
                    (x, supGroup) => new { x.we, x.si, x.proj, supGroup })
                .SelectMany(x => x.supGroup.DefaultIfEmpty(),
                    (x, sup) => new { x.we, x.si, x.proj, sup })

                .GroupJoin(_context.Parties,
                    x => x.we.PartyIdContractor,
                    p => p.PartyId,
                    (x, conGroup) => new { x.we, x.si, x.proj, x.sup, conGroup })
                .SelectMany(x => x.conGroup.DefaultIfEmpty(),
                    (x, con) => new { x.we, x.si, x.proj, x.sup, con })

                .GroupJoin(_context.Facilities,
                    x => x.we.FacilityId,
                    f => f.FacilityId,
                    (x, facGroup) => new { x.we, x.si, x.proj, x.sup, x.con, facGroup })
                .SelectMany(x => x.facGroup.DefaultIfEmpty(),
                    (x, fac) => new { x.we, x.si, x.proj, x.sup, x.con, fac });

            // REFACTOR: LEFT JOIN to CERTIFICATE_ITEM rows (may be empty)
            // Purpose: Enables per-certificate total calculation without losing certificates
            var withItems = joined
                .GroupJoin(
                    _context.WorkEfforts
                        .AsNoTracking()
                        .Where(i => i.WorkEffortTypeId == "CERTIFICATE_ITEM"),
                    cert => cert.we.WorkEffortId,
                    item => item.WorkEffortParentId,
                    (cert, items) => new { cert, items })
                .SelectMany(
                    x => x.items.DefaultIfEmpty(),
                    (x, item) => new { x.cert.we, x.cert.si, x.cert.proj, x.cert.sup, x.cert.con, x.cert.fac, item });

            // REFACTOR: Group by certificate key and compute the **exact same** totals as ListCertificateItems
            // Purpose: Guarantees UI consistency – Total = Net for WORKMANSHIP, otherwise legacy supply formula
            var result = await withItems
                .GroupBy(g => new
                {
                    g.we.WorkEffortId,
                    g.we.CertificateNumber,
                    g.we.CertificateCategory,
                    g.we.ProjectId,
                    ProjectName = g.proj != null ? g.proj.ProjectName : g.we.ProjectName,
                    g.we.Description,
                    g.we.EstimatedStartDate,
                    g.we.EstimatedCompletionDate,
                    g.we.CurrentStatusId,
                    StatusDescription = lang == "ar"
                        ? g.si.DescriptionArabic ?? "غير معروف"
                        : g.si.Description ?? "Unknown",
                    StatusDescriptionArabic = g.si.DescriptionArabic ?? "غير معروف",
                    g.we.PartyIdSupplier,
                    SupplierName = g.sup.Description,
                    g.we.PartyIdContractor,
                    ContractorName = g.con.Description,
                    g.we.FacilityId,
                    FacilityName = g.fac.FacilityName
                })
                .Select(g => new ProjectCertificateSummaryDto
                {
                    WorkEffortId               = g.Key.WorkEffortId,
                    CertificateNumber          = g.Key.CertificateNumber ?? string.Empty,
                    CertificateCategory        = g.Key.CertificateCategory ?? string.Empty,
                    CertificateCategoryDescription = GetCertificateCategoryDescription(g.Key.CertificateCategory),
                    ProjectId                  = g.Key.ProjectId ?? string.Empty,
                    ProjectName                = g.Key.ProjectName ?? string.Empty,
                    Description                = g.Key.Description ?? string.Empty,
                    EstimatedStartDate         = g.Key.EstimatedStartDate,
                    EstimatedCompletionDate    = g.Key.EstimatedCompletionDate,
                    StatusDescription          = g.Key.StatusDescription,
                    StatusDescriptionArabic    = g.Key.StatusDescriptionArabic,
                    CurrentStatusId            = g.Key.CurrentStatusId ?? string.Empty,
                    PartyIdSupplier            = g.Key.PartyIdSupplier,
                    PartyNameSupplier          = g.Key.SupplierName,
                    PartyIdContractor          = g.Key.PartyIdContractor,
                    PartyNameContractor        = g.Key.ContractorName,
                    FacilityId                 = g.Key.FacilityId,
                    FacilityName               = g.Key.FacilityName,

                    // REFACTOR: Total = Net amount (matches UI "Net" column)
                    // Purpose: Use the same business rules as ListCertificateItems
                    //   WORKMANSHIP:  Qty * (Mat + Lab) * Ach% – Deductions – Ins – AddIns
                    //   LEGACY:       Qty * Rate + Trans + Grat – Discount
                    Total = Math.Round(
                        g.Key.CertificateCategory == "WORKMANSHIP_CONTRACTING_CERTIFICATE"
                            ? g.Sum(i => i.item == null ? 0m :
                                  Math.Max(0m,
                                      ((i.item.Quantity ?? 0m) *
                                       ((i.item.MaterialPrice ?? 0m) + (i.item.LaborPrice ?? 0m)) *
                                       ((decimal)(i.item.AchievementPercent ?? 0) / 100m)) -
                                      (i.item.Deductions ?? 0m) -
                                      (i.item.Insurance ?? 0m) -
                                      (i.item.AdditionalInsurance ?? 0m)))
                            : g.Sum(i => i.item == null ? 0m :
                                  ((i.item.Quantity ?? 0m) * (i.item.Rate ?? 0m)) +
                                  (i.item.TransportationExpenses ?? 0m) +
                                  (i.item.Gratuities ?? 0m) -
                                  (i.item.Discount ?? 0m)),
                        2)
                })
                .ToListAsync(ct);

            return Result<List<ProjectCertificateSummaryDto>>.Success(result);
        }

        // REFACTOR: Helper moved to static method (pure function, easy to unit-test)
        // Purpose: Keep projection clean and reusable
        private static string GetCertificateCategoryDescription(string? category) => category switch
        {
            "SUPPLY_PROCUREMENT_CERTIFICATE"      => "Supply Procurement Certificate",
            "WORKMANSHIP_CONTRACTING_CERTIFICATE" => "Workmanship Contracting Certificate",
            "COMPANY_SUPPLY_SALE_CERTIFICATE"     => "Company Supply Sale Certificate",
            _                                     => "Unknown Certificate"
        };
    }
}