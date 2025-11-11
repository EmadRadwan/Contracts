.Select(x => new CertificateItemDto
{
    WorkEffortId = x.WorkEffort.WorkEffortId,
    WorkEffortParentId = x.WorkEffort.WorkEffortParentId,
    ProductId = x.WorkEffort.ProductId,
    ProductIdObject = x.Product != null
        ? new ProductLovDto { ProductId = x.Product.ProductId, ProductName = x.Product.ProductName }
        : null,
    UomId = x.WorkEffort.QuantityUomId,
    QuantityUomObject = x.Uom != null
        ? new UomLovDto { UomId = x.Uom.UomId, Description = x.Uom.Description }
        : null,
    Description = x.WorkEffort.Description,
    ProductName = x.Product?.ProductName,
    UomName = x.Uom != null
        ? (language == "ar" ? x.Uom.DescriptionArabic : x.Uom.Description)
        : null,
    Quantity = x.WorkEffort.Quantity ?? 0m,
    UnitPrice = (decimal)(x.WorkEffort.Rate ?? 0m),
    MaterialPrice = x.WorkEffort.MaterialPrice ?? 0m,
    LaborPrice = x.WorkEffort.LaborPrice ?? 0m,

    // REFACTOR: All calculations inline – EF Core compatible
    // Purpose: Compute on-the-fly, no stored values
    TotalAmount = (x.Parent.CertificateCategory == "WORKMANSHIP_CONTRACTING_CERTIFICATE")
        ? (x.WorkEffort.Quantity ?? 0m) * ((x.WorkEffort.MaterialPrice ?? 0m) + (x.WorkEffort.LaborPrice ?? 0m))
        : (x.WorkEffort.TotalAmount ?? 
           ((x.WorkEffort.Quantity ?? 0m) * (x.WorkEffort.Rate ?? 0m)) +
           (x.WorkEffort.TransportationExpenses ?? 0m) +
           (x.WorkEffort.Gratuities ?? 0m) -
           (x.WorkEffort.Discount ?? 0m)),

    Deserved = (x.Parent.CertificateCategory == "WORKMANSHIP_CONTRACTING_CERTIFICATE")
        ? Math.Max(0m,
            ((x.WorkEffort.Quantity ?? 0m) * ((x.WorkEffort.MaterialPrice ?? 0m) + (x.WorkEffort.LaborPrice ?? 0m)) *
             ((decimal)(x.WorkEffort.AchievementPercent ?? 0) / 100m)) -
            (x.WorkEffort.Deductions ?? 0m))
        : 0m,

    Net = (x.Parent.CertificateCategory == "WORKMANSHIP_CONTRACTING_CERTIFICATE")
        ? Math.Max(0m,
            (Math.Max(0m,
                ((x.WorkEffort.Quantity ?? 0m) * ((x.WorkEffort.MaterialPrice ?? 0m) + (x.WorkEffort.LaborPrice ?? 0m)) *
                 ((decimal)(x.WorkEffort.AchievementPercent ?? 0) / 100m)) -
                (x.WorkEffort.Deductions ?? 0m))) -
            (x.WorkEffort.Insurance ?? 0m) -
            (x.WorkEffort.AdditionalInsurance ?? 0m))
        : (x.WorkEffort.TotalAmount ?? 
           ((x.WorkEffort.Quantity ?? 0m) * (x.WorkEffort.Rate ?? 0m)) +
           (x.WorkEffort.TransportationExpenses ?? 0m) +
           (x.WorkEffort.Gratuities ?? 0m) -
           (x.WorkEffort.Discount ?? 0m)),

    Discount = x.WorkEffort.Discount ?? 0m,
    Insurance = x.WorkEffort.Insurance ?? 0m,
    AdditionalInsurance = x.WorkEffort.AdditionalInsurance ?? 0m,
    CompletionPercentage = x.WorkEffort.CompletionPercentage,
    Notes = x.WorkEffort.Notes,
    ProcurementDate = x.WorkEffort.ProcurementDate ?? x.WorkEffort.CreatedDate,
    IsDeleted = false,
    AchievementPercentage = x.WorkEffort.AchievementPercent,
    TransportationExpenses = x.WorkEffort.TransportationExpenses ?? 0m,
    Gratuities = x.WorkEffort.Gratuities ?? 0m,
    Deductions = x.WorkEffort.Deductions ?? 0m,
    DeductionDescription = x.WorkEffort.DeductionDescription
})