// REFACTOR: Updated CertificateItemDto to align with WorkEffort entity properties.
// Purpose: Use exact prop names from WorkEffort (e.g., DiscountAmount -> discount, TransportationExpenses -> transportationExpenses); make all relevant fields nullable to match sparse CSV data.
// Improvement: Ensures direct mapping without mismatches; supports saved non-zero values (e.g., Gratuities=0.450) by leveraging entity props, reducing frontend coercion needs.
// Context: Based on provided WorkEffort model (snake_case JSON) and CSV analysis (82 cols, non-empties in ~col 70-76 map to AchievementPercent, DueAmount, PaidAmount, Deductions, Gratuities, etc.); query now populates from entity.
public class CertificateItemDto
{
    public string WorkEffortId { get; set; }
    public string WorkEffortParentId { get; set; }
    public string ProductId { get; set; }
    public ProductLovDto ProductIdObject { get; set; }
    public string QuantityUom { get; set; }
    public UomLovDto QuantityUomObject { get; set; }
    public string Description { get; set; }
    public string ProductName { get; set; }
    public string UomDescription { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? Rate { get; set; }  // Maps to UnitPrice in frontend
    public decimal TotalAmount { get; set; }
    public decimal? DiscountAmount { get; set; }  // Maps to discount
    public decimal? InsuranceAmount { get; set; }  // Maps to insurance
    public decimal? CompletionPercentage { get; set; }
    public string Notes { get; set; }
    public DateTime? ProcurementDate { get; set; }
    public string FacilityId { get; set; }
    public string FacilityName { get; set; }
    public bool IsDeleted { get; set; }
    public decimal? AchievementPercent { get; set; }  // Maps to achievementPercentage; e.g., CSV col 70: "3.000000"
    public decimal? TransportationExpenses { get; set; }  // e.g., CSV col ~74-75: "3.000" in row 2
    public decimal? Gratuities { get; set; }  // e.g., CSV col 76: "0.450" in row 2
    public decimal? Deductions { get; set; }  // e.g., CSV col 75?: "0.000" defaults
    public decimal? DueAmount { get; set; }  // Maps to deserved (calculated if null)
    public decimal? PaidAmount { get; set; }  // Maps to net (calculated if null)
    public decimal? RemainingAmount { get; set; }  // Optional, for future use
    public bool? IsContractorPurchased { get; set; }  // Default false; not in CSV but kept
}

// REFACTOR: Updated Handler query to map all relevant WorkEffort properties directly.
// Purpose: Populate type-specific fields from entity (e.g., Gratuities = we.Gratuities ?? 0); use fallbacks for calculated fields (DueAmount, PaidAmount) based on CSV sparsity.
// Improvement: Captures saved values accurately (e.g., Gratuities=0.450 from entity prop matching CSV col 76), ensuring fetch reflects DB state without artificial nulls; efficient EF projection.
// Context: WorkEffort model confirms props like TransportationExpenses, Gratuities, Deductions exist post "// new props for the Contracts system"; CSV non-empties align (e.g., col 71="3.000" -> AchievementPercent?); calculations for deserved/net use entity values or derive if null.
public async Task<Result<List<CertificateItemDto>>> Handle(Query request, CancellationToken cancellationToken)
{
    // ... validation code unchanged ...

    try
    {
        var certificateItems = await _context.WorkEfforts
            .Where(we => we.WorkEffortParentId == request.WorkEffortId && we.WorkEffortTypeId == "CERTIFICATE_ITEM")
            .GroupJoin(
                _context.Products,
                we => we.ProductId,
                prd => prd.ProductId,
                (we, prdGroup) => new { WorkEffort = we, Products = prdGroup }
            )
            .SelectMany(
                x => x.Products.DefaultIfEmpty(),
                (x, prd) => new { x.WorkEffort, Product = prd }
            )
            .GroupJoin(
                _context.Uoms,
                x => x.WorkEffort.QuantityUomId,
                uom => uom.UomId,
                (x, uomGroup) => new { x.WorkEffort, x.Product, Uoms = uomGroup }
            )
            .SelectMany(
                x => x.Uoms.DefaultIfEmpty(),
                (x, uom) => new { x.WorkEffort, x.Product, Uom = uom }
            )
            .GroupJoin(
                _context.Facilities,
                x => x.WorkEffort.FacilityId,
                fac => fac.FacilityId,
                (x, facGroup) => new { x.WorkEffort, x.Product, x.Uom, Facilities = facGroup }
            )
            .SelectMany(
                x => x.Facilities.DefaultIfEmpty(),
                (x, fac) => new CertificateItemDto
                {
                    WorkEffortId = x.WorkEffort.WorkEffortId,
                    WorkEffortParentId = x.WorkEffort.WorkEffortParentId,
                    ProductId = x.WorkEffort.ProductId,
                    ProductIdObject = x.Product != null ? new ProductLovDto
                    {
                        ProductId = x.Product.ProductId,
                        ProductName = x.Product.ProductName
                    } : null,
                    QuantityUom = x.WorkEffort.QuantityUomId,
                    QuantityUomObject = x.Uom != null ? new UomLovDto
                    {
                        UomId = x.Uom.UomId,
                        Description = x.Uom.Description
                    } : null,
                    Description = x.WorkEffort.Description,
                    ProductName = x.Product != null ? x.Product.ProductName : null,
                    UomDescription = x.Uom != null ? x.Uom.Description : null,
                    Quantity = x.WorkEffort.Quantity,
                    Rate = x.WorkEffort.Rate,  // Maps to unitPrice
                    TotalAmount = x.WorkEffort.TotalAmount ?? ((x.WorkEffort.Quantity ?? 0m) * (x.WorkEffort.Rate ?? 0m)),
                    DiscountAmount = x.WorkEffort.DiscountAmount,
                    InsuranceAmount = x.WorkEffort.InsuranceAmount,
                    CompletionPercentage = x.WorkEffort.CompletionPercentage,
                    Notes = x.WorkEffort.Notes,
                    ProcurementDate = x.WorkEffort.ProcurementDate ?? x.WorkEffort.CreatedDate,  // Fallback as per CSV timestamps
                    FacilityId = x.WorkEffort.FacilityId,
                    FacilityName = fac?.FacilityName ?? "",
                    IsDeleted = false,
                    // REFACTOR: Direct mapping for contracts-specific props from WorkEffort.
                    // Purpose: Fetch saved values (e.g., Gratuities=0.450 from col 76 equivalent) without null defaults.
                    // Improvement: Aligns with entity schema; uses null-coalescing for calculations to handle sparsity.
                    // Context: CSV shows non-zeros in late cols (e.g., col 76="0.450" -> Gratuities); AchievementPercent ~col 70="3.000000".
                    AchievementPercent = x.WorkEffort.AchievementPercent,
                    TransportationExpenses = x.WorkEffort.TransportationExpenses,
                    Gratuities = x.WorkEffort.Gratuities,  // Captures saved 0.450
                    Deductions = x.WorkEffort.Deductions,
                    DueAmount = x.WorkEffort.DueAmount ?? ((x.WorkEffort.Quantity ?? 0m) * (x.WorkEffort.Rate ?? 0m) - (x.WorkEffort.Deductions ?? 0m)),  // Deserved fallback
                    PaidAmount = x.WorkEffort.PaidAmount ?? (x.WorkEffort.DueAmount ?? 0m) - (x.WorkEffort.InsuranceAmount ?? 0m),  // Net fallback
                    RemainingAmount = x.WorkEffort.RemainingAmount,
                    IsContractorPurchased = x.WorkEffort.SupplierOrContractorType == "CONTRACTOR"  // Derive from type if needed; default false otherwise
                }
            )
            .ToListAsync(cancellationToken);

        return Result<List<CertificateItemDto>>.Success(certificateItems);
    }
    catch (Exception ex)
    {
        // ... error handling unchanged ...
    }
}