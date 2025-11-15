using Application.Core;
using Application.Interfaces;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Order.SalesRequests;

public class CreateSalesRequest
{
    // -----------------------------------------------------------------
    // Command
    // -----------------------------------------------------------------
    public class Command : IRequest<Result<SalesRequestResponseDto>>
    {
        public SalesRequestDto? SalesRequestDto { get; set; }
    }

    // -----------------------------------------------------------------
    // Flat Response DTO – all scalar values, no nested objects
    // -----------------------------------------------------------------
    public class SalesRequestResponseDto
    {
        public string SalesRequestId { get; set; } = string.Empty;

        // FromParty (flattened)
        public string FromPartyId { get; set; } = string.Empty;
        public string FromPartyName { get; set; } = string.Empty;
        public string FromPartyPhone { get; set; } = string.Empty;

        // Apartment (flattened) – matches GetSimpleApartmentsLov.ApartmentLovDto
        public string ApartmentId { get; set; } = string.Empty;
        public string ApartmentName { get; set; } = string.Empty;
        public string ApartmentType { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public string FloorNumber { get; set; } = string.Empty;
        public decimal ApartmentSpaceM2 { get; set; }
        public decimal? GardenSpaceM2 { get; set; }
        public decimal? GardenPricePerM2 { get; set; }
        public decimal ApartmentPricePerM2 { get; set; }
        public string ApartmentStatusId { get; set; } = string.Empty;
        public string ApartmentStatusDescription { get; set; } = string.Empty;

        // Pricing & Payment
        public decimal TotalPrice { get; set; }
        public decimal? Discount { get; set; }
        public decimal AdvancePayment { get; set; }
        public int NumberOfInstallments { get; set; }
        public DateTime? DateOfFirstInstallment { get; set; }
        public int DurationBetweenInstallments { get; set; }

        // Metadata
        public DateTime SaleDate { get; set; }
        public string? Comments { get; set; }
        public DateTime CreatedStamp { get; set; }
        public DateTime LastUpdatedStamp { get; set; }
    }

    // -----------------------------------------------------------------
    // Strongly-typed projection for apartment LOV (replaces dynamic)
    // -----------------------------------------------------------------
    public class ApartmentLovProjection
    {
        public string ApartmentId { get; set; } = string.Empty;
        public string ApartmentName { get; set; } = string.Empty;
        public string ApartmentType { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public string FloorNumber { get; set; } = string.Empty;
        public decimal ApartmentSpaceM2 { get; set; }
        public decimal? GardenSpaceM2 { get; set; }
        public decimal? GardenPricePerM2 { get; set; }
        public decimal ApartmentPricePerM2 { get; set; }
        public string ApartmentStatusId { get; set; } = string.Empty;
        public string ApartmentStatusDescription { get; set; } = string.Empty;
    }

    // -----------------------------------------------------------------
    // Handler
    // -----------------------------------------------------------------
    public class Handler : IRequestHandler<Command, Result<SalesRequestResponseDto>>
    {
        private readonly DataContext _context;
        private readonly IUtilityService _utilityService;
        private readonly IUserAccessor _userAccessor;

        public Handler(DataContext context, IUserAccessor userAccessor, IUtilityService utilityService)
        {
            _context = context;
            _userAccessor = userAccessor;
            _utilityService = utilityService;
        }

        public async Task<Result<SalesRequestResponseDto>> Handle(Command request, CancellationToken ct)
        {
            var dto = request.SalesRequestDto!;

            // -----------------------------------------------------------------
            // 1. User validation
            // -----------------------------------------------------------------
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == _userAccessor.GetUsername(), ct);

            if (user == null)
                return Result<SalesRequestResponseDto>.Failure("User not found");

            // -----------------------------------------------------------------
            // 2. Generate SalesRequestId
            // -----------------------------------------------------------------
            var salesRequestId = await _utilityService.GetNextSequence("SalesRequest");
            var now = DateTime.UtcNow;

            // REFACTOR: Use UTC for consistency across servers and time zones

            // -----------------------------------------------------------------
            // 3. Transaction scope
            // -----------------------------------------------------------------
            await using var transaction = await _context.Database.BeginTransactionAsync(ct);

            try
            {
                // -----------------------------------------------------------------
                // 4. Create and persist SalesRequest
                // -----------------------------------------------------------------
                var sr = new SalesRequest
                {
                    SalesRequestId = salesRequestId,
                    ProductId = dto.ProductId!,
                    SaleDate = dto.SaleDate!.Value,
                    FromPartyId = dto.FromPartyId!,
                    ApartmentPricePerM2 = dto.ApartmentPricePerM2,
                    GardenPricePerM2 = dto.GardenPricePerM2,
                    Discount = dto.Discount,
                    TotalPrice = dto.TotalPrice,
                    AdvancePayment = dto.AdvancePayment,
                    NumberOfInstallments = dto.NumberOfInstallments,
                    DateOfFirstInstallment = dto.DateOfFirstInstallment,
                    DurationBetweenInstallments = dto.DurationBetweenInstallments,
                    Comments = dto.Comments,
                    CreatedStamp = now,
                    LastUpdatedStamp = now
                };

                _context.SalesRequests.Add(sr);

                var saved = await _context.SaveChangesAsync(ct) > 0;
                if (!saved)
                {
                    await transaction.RollbackAsync(ct);
                    return Result<SalesRequestResponseDto>.Failure("Failed to create sales request");
                }

                await transaction.CommitAsync(ct);

                // -----------------------------------------------------------------
                // 5. Load FromParty details (name only – phone not available)
                // -----------------------------------------------------------------
                // REFACTOR: Fetch party name in one query; phone not in Party table → leave empty
                var fromParty = await _context.Parties
                    .Where(p => p.PartyId == dto.FromPartyId)
                    .Select(p => new
                    {
                        p.PartyId,
                        p.Description,
                        Phone = string.Empty
                    })
                    .FirstOrDefaultAsync(ct);

                // -----------------------------------------------------------------
                // 6. Reuse GetSimpleApartmentsLov logic for apartment data
                // -----------------------------------------------------------------
                // REFACTOR: Extract reusable projection logic to avoid duplication
                //         Ensures 100% consistency with LOV dropdowns
                var apartment = await GetApartmentLovProjection(_context, dto.ProductId!, ct);

                if (apartment == null)
                {
                    // Fallback: use input values if product not found (shouldn't happen)
                    // REFACTOR: Defensive fallback to prevent null reference
                    apartment = new ApartmentLovProjection
                    {
                        ApartmentId = dto.ProductId!,
                        ApartmentName = string.Empty,
                        ApartmentType = "APARTMENT",
                        ProjectName = string.Empty,
                        FloorNumber = string.Empty,
                        ApartmentSpaceM2 = 0m,
                        GardenSpaceM2 = null,
                        GardenPricePerM2 = dto.GardenPricePerM2,
                        ApartmentPricePerM2 = (decimal)dto.ApartmentPricePerM2,
                        ApartmentStatusId = string.Empty,
                        ApartmentStatusDescription = string.Empty
                    };
                }

                // -----------------------------------------------------------------
                // 7. Build flat response DTO
                // -----------------------------------------------------------------
                var response = new SalesRequestResponseDto
                {
                    SalesRequestId = salesRequestId,

                    // FromParty
                    FromPartyId = fromParty?.PartyId ?? dto.FromPartyId!,
                    FromPartyName = fromParty?.Description ?? string.Empty,
                    FromPartyPhone = fromParty?.Phone ?? string.Empty,

                    // Apartment – fully consistent with LOV
                    ApartmentId = apartment.ApartmentId,
                    ApartmentName = apartment.ApartmentName,
                    ApartmentType = apartment.ApartmentType,
                    ProjectName = apartment.ProjectName,
                    FloorNumber = apartment.FloorNumber,
                    ApartmentSpaceM2 = apartment.ApartmentSpaceM2,
                    GardenSpaceM2 = apartment.GardenSpaceM2,
                    GardenPricePerM2 = apartment.GardenPricePerM2 ?? dto.GardenPricePerM2,
                    ApartmentPricePerM2 = apartment.ApartmentPricePerM2,
                    ApartmentStatusId = apartment.ApartmentStatusId,
                    ApartmentStatusDescription = apartment.ApartmentStatusDescription,

                    // Payment & Pricing
                    TotalPrice = (decimal)dto.TotalPrice,
                    Discount = dto.Discount,
                    AdvancePayment = (decimal)dto.AdvancePayment,
                    NumberOfInstallments = (int)dto.NumberOfInstallments,
                    DateOfFirstInstallment = dto.DateOfFirstInstallment,
                    DurationBetweenInstallments = (int)dto.DurationBetweenInstallments,

                    // Metadata
                    SaleDate = dto.SaleDate!.Value,
                    Comments = dto.Comments,
                    CreatedStamp = now,
                    LastUpdatedStamp = now
                };

                return Result<SalesRequestResponseDto>.Success(response);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                return Result<SalesRequestResponseDto>.Failure($"Failed to create sales request: {ex.Message}");
            }
        }

        // -----------------------------------------------------------------
        // Reusable method: mirrors GetSimpleApartmentsLov projection
        // -----------------------------------------------------------------
        // REFACTOR: Extracted to avoid code duplication and ensure consistency
        //         between LOV dropdown and create response
        // REFACTOR: Fixed EF Core translation error by moving TryGetValue(out var) outside query
        //         Now: fetch raw data → apply lookups in-memory → return strong-typed result
        public static async Task<ApartmentLovProjection?> GetApartmentLovProjection(
            DataContext context, string productId, CancellationToken ct)
        {
            // -----------------------------------------------------------------
            // 1. Load lookup dictionaries (in-memory)
            // -----------------------------------------------------------------
            var projectNameLookup = await context.WorkEfforts
                .Where(w => w.WorkEffortTypeId == "PROJECT")
                .GroupBy(w => w.WorkEffortId)
                .Select(g => new
                {
                    ProjectId = g.Key,
                    ProjectName = g.OrderByDescending(w => w.WorkEffortId)
                                   .Select(w => w.ProjectName)
                                   .FirstOrDefault()
                })
                .ToDictionaryAsync(x => x.ProjectId, x => x.ProjectName ?? "", ct);

            var statusLookup = await context.StatusItems
                .Where(s => s.StatusTypeId == "APARTMENT_STATUS")
                .ToDictionaryAsync(s => s.StatusId, s => s.Description ?? s.StatusId, ct);

            var floorMap = new Dictionary<string, string>
            {
                { "0", "الطابق الأرضي" },
                { "1", "الطابق الأول" },
                { "2", "الطابق الثاني" },
                { "3", "الطابق الثالث" },
                { "4", "الطابق الرابع" },
                { "5", "الطابق الخامس" },
                { "6", "الطابق السادس" }
            };

            // -----------------------------------------------------------------
            // 2. Query product with only translatable fields
            // -----------------------------------------------------------------
            var rawApartment = await context.Products
                .Where(p => p.ProductId == productId && p.ProductTypeId == "APARTMENT")
                .Select(p => new
                {
                    ApartmentId = p.ProductId,
                    ApartmentName = p.ProductName,
                    ApartmentType = p.ProductTypeId,
                    ProjectId = p.ProjectId,
                    FloorNumber = p.FloorNumber,
                    ApartmentSpaceM2 = p.ApartmentSpaceM2,
                    GardenSpaceM2 = p.GardenSpaceM2,
                    GardenPricePerM2 = p.GardenPricePerM2,
                    ApartmentPricePerM2 = p.ApartmentPricePerM2,
                    ApartmentStatusId = p.ApartmentStatusId
                })
                .FirstOrDefaultAsync(ct);

            if (rawApartment == null) return null;

            // -----------------------------------------------------------------
            // 3. Apply lookups in-memory (safe – no EF translation)
            // -----------------------------------------------------------------
            // REFACTOR: Perform TryGetValue post-materialization to avoid expression tree errors
            var projectName = rawApartment.ProjectId != null && 
                            projectNameLookup.TryGetValue(rawApartment.ProjectId, out var pn)
                ? pn
                : "";

            var statusDesc = rawApartment.ApartmentStatusId != null && 
                           statusLookup.TryGetValue(rawApartment.ApartmentStatusId, out var desc)
                ? desc
                : rawApartment.ApartmentStatusId ?? "";

            var floorLabel = rawApartment.FloorNumber != null && 
                           floorMap.TryGetValue(rawApartment.FloorNumber, out var fn)
                ? fn
                : rawApartment.FloorNumber ?? "";

            // -----------------------------------------------------------------
            // 4. Build final strongly-typed projection
            // -----------------------------------------------------------------
            return new ApartmentLovProjection
            {
                ApartmentId = rawApartment.ApartmentId,
                ApartmentName = rawApartment.ApartmentName,
                ApartmentType = rawApartment.ApartmentType,
                ProjectName = projectName,
                FloorNumber = floorLabel,
                ApartmentSpaceM2 = rawApartment.ApartmentSpaceM2 ?? 0m,
                GardenSpaceM2 = rawApartment.GardenSpaceM2,
                GardenPricePerM2 = rawApartment.GardenPricePerM2,
                ApartmentPricePerM2 = rawApartment.ApartmentPricePerM2 ?? 0m,
                ApartmentStatusId = rawApartment.ApartmentStatusId ?? "",
                ApartmentStatusDescription = statusDesc
            };
        }
    }
}