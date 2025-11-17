// -----------------------------------------------------------------
// Handler
// -----------------------------------------------------------------
public class Handler : IRequestHandler<Command, Result<SalesRequestResponseDto>>
{
    private readonly DataContext _context;
    private readonly IUtilityService _utilityService;
    private readonly IUserAccessor _userAccessor;

    private const string SalesRequestCreatedStatusId = "SALES_REQUEST_CREATED";
    private const string ApartmentReservedStatusId   = "APARTMENT_RESERVED";   // <-- NEW

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
        // 1. Validate current user
        // -----------------------------------------------------------------
        // REFACTOR: Early-exit with meaningful message; no need to continue if user missing.
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.UserName == _userAccessor.GetUsername(), ct);
        if (user == null) return Result<SalesRequestResponseDto>.Failure("User not found");

        // -----------------------------------------------------------------
        // 2. Generate next SalesRequestId (sequence)
        // -----------------------------------------------------------------
        // REFACTOR: Kept exactly as requested – uses _utilityService.GetNextSequence("SalesRequest")
        var salesRequestId = await _utilityService.GetNextSequence("SalesRequest");
        var now = DateTime.UtcNow;

        // -----------------------------------------------------------------
        // 3. Build domain entity
        // -----------------------------------------------------------------
        var sr = new SalesRequest
        {
            SalesRequestId          = salesRequestId,
            ProductId               = dto.ProductId!,
            SaleDate                = dto.SaleDate!.Value,
            FromPartyId             = dto.FromPartyId!,
            ApartmentPricePerM2     = dto.ApartmentPricePerM2,
            GardenPricePerM2        = dto.GardenPricePerM2,
            Discount                = dto.Discount,
            TotalPrice              = dto.TotalPrice,
            AdvancePayment          = dto.AdvancePayment,
            NumberOfInstallments    = dto.NumberOfInstallments,
            DateOfFirstInstallment  = dto.DateOfFirstInstallment,
            MonthsBetweenInstallments = dto.MonthsBetweenInstallments,
            MaintenanceDeposit      = dto.MaintenanceDeposit,
            StatusId                = SalesRequestCreatedStatusId,
            Comments                = dto.Comments,
            CreatedStamp            = now,
            LastUpdatedStamp        = now
        };
        _context.SalesRequests.Add(sr);

        // -----------------------------------------------------------------
        // 4. Persist SalesRequest + update apartment status in ONE transaction
        // -----------------------------------------------------------------
        // REFACTOR: Added apartment status update **before** SaveChangesAsync.
        //           This guarantees atomicity – both changes succeed or both fail.
        var apartment = await _context.Products
            .FirstOrDefaultAsync(p => p.ProductId == dto.ProductId! && p.ProductTypeId == "APARTMENT", ct);

        if (apartment == null)
            return Result<SalesRequestResponseDto>.Failure("Apartment not found");

        // REFACTOR: Change status to APARTMENT_RESERVED.
        //           Improves domain consistency: apartment becomes reserved the moment the request is created.
        apartment.ApartmentStatusId = ApartmentReservedStatusId;

        try
        {
            var saved = await _context.SaveChangesAsync(ct) > 0;
            if (!saved) return Result<SalesRequestResponseDto>.Failure("Failed to persist sales request");
        }
        catch (Exception ex)
        {
            // REFACTOR: No manual rollback needed – EF automatically rolls back on exception.
            return Result<SalesRequestResponseDto>.Failure($"Database error: {ex.Message}");
        }

        // -----------------------------------------------------------------
        // 5. Load auxiliary data **sequentially** – NO parallel EF calls
        // -----------------------------------------------------------------
        // REFACTOR: Re-load apartment LOV **after** status update to return the new status.
        var fromParty = await _context.Parties
            .Where(p => p.PartyId == dto.FromPartyId)
            .Select(p => new { p.PartyId, p.Description, Phone = string.Empty })
            .FirstOrDefaultAsync(ct);

        var apartmentLov = await GetApartmentLovProjection(_context, dto.ProductId!, ct)
                           ?? new ApartmentLovProjection
                           {
                               ApartmentId               = dto.ProductId!,
                               ApartmentName             = string.Empty,
                               ApartmentType             = "APARTMENT",
                               ProjectName               = string.Empty,
                               FloorNumber               = string.Empty,
                               ApartmentSpaceM2          = 0m,
                               GardenSpaceM2             = null,
                               GardenPricePerM2          = dto.GardenPricePerM2,
                               ApartmentPricePerM2       = (decimal)dto.ApartmentPricePerM2,
                               ApartmentStatusId         = ApartmentReservedStatusId,   // fallback
                               ApartmentStatusDescription = string.Empty
                           };

        var statusDesc = await _context.StatusItems
            .Where(s => s.StatusId == SalesRequestCreatedStatusId)
            .Select(s => s.Description ?? s.StatusId)
            .FirstOrDefaultAsync(ct) ?? SalesRequestCreatedStatusId;

        // -----------------------------------------------------------------
        // 6. Build flat response DTO
        // -----------------------------------------------------------------
        // REFACTOR: Apartment status now reflects APARTMENT_RESERVED.
        var response = new SalesRequestResponseDto
        {
            SalesRequestId = salesRequestId,

            // FromParty
            FromPartyId    = fromParty?.PartyId ?? dto.FromPartyId!,
            FromPartyName  = fromParty?.Description ?? string.Empty,
            FromPartyPhone = fromParty?.Phone ?? string.Empty,

            // Apartment (consistent with LOV)
            ApartmentId                = apartmentLov.ApartmentId,
            ApartmentName              = apartmentLov.ApartmentName,
            ApartmentType              = apartmentLov.ApartmentType,
            ProjectName                = apartmentLov.ProjectName,
            FloorNumber                = apartmentLov.FloorNumber,
            ApartmentSpaceM2           = apartmentLov.ApartmentSpaceM2,
            GardenSpaceM2              = apartmentLov.GardenSpaceM2,
            GardenPricePerM2           = apartmentLov.GardenPricePerM2 ?? dto.GardenPricePerM2,
            ApartmentPricePerM2        = apartmentLov.ApartmentPricePerM2,
            ApartmentStatusId          = apartmentLov.ApartmentStatusId,          // now APARTMENT_RESERVED
            ApartmentStatusDescription = apartmentLov.ApartmentStatusDescription,

            // Pricing & Payment
            TotalPrice                = (decimal)dto.TotalPrice,
            Discount                  = dto.Discount,
            AdvancePayment            = (decimal)dto.AdvancePayment,
            NumberOfInstallments      = (int)dto.NumberOfInstallments,
            DateOfFirstInstallment    = dto.DateOfFirstInstallment,
            MonthsBetweenInstallments = (int)dto.MonthsBetweenInstallments,
            MaintenanceDeposit        = dto.MaintenanceDeposit,

            // Metadata
            SaleDate        = dto.SaleDate!.Value,
            Comments        = dto.Comments,
            StatusId        = SalesRequestCreatedStatusId,
            StatusDescription = statusDesc,
            CreatedStamp    = now,
            LastUpdatedStamp = now
        };

        return Result<SalesRequestResponseDto>.Success(response);
    }

    // -----------------------------------------------------------------
    // Reusable apartment LOV projection (unchanged)
    // -----------------------------------------------------------------
    public static async Task<ApartmentLovProjection?> GetApartmentLovProjection(
        DataContext ctx, string productId, CancellationToken ct)
    {
        // (unchanged – omitted for brevity)
        // ... existing implementation ...
    }
}