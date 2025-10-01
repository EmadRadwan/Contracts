namespace Application.Projects;

public class Certificate
{
    public string CertificateNumber { get; set; }
    public string Description { get; set; }
    public string PartyIdSupplier { get; set; }
    public string PartyIdContractor { get; set; }
    public string FacilityName { get; set; }
    public string CurrentStatusId { get; set; } // E.g., CREATED
}

public class CertificateItem
{
    public string ProductName { get; set; }
    public string Code { get; set; }
    public string Description { get; set; }
    public double? Quantity { get; set; }
    public string UomName { get; set; }
    public double? UnitPrice { get; set; } // For supply certificates
    public double? MaterialPrice { get; set; } // For workmanship certificates
    public double? LaborPrice { get; set; } // For workmanship certificates
    public double? DisplayTotal { get; set; }
    public double? Discount { get; set; } // For supply with discount
    public string FormattedProcurementDate { get; set; } // For supply certificates
    public double? TransportationExpenses { get; set; } // For supply certificates
    public double? Gratuities { get; set; } // For supply certificates
    public bool IsLastInGroup { get; set; }
    public double? ProductSubtotal { get; set; }
    public string MainItemDescription { get; set; }
    public string DiscountNote { get; set; }
    public double? Deductions { get; set; } // For workmanship certificates
    public string DeductionDescription { get; set; } // For workmanship certificates
    public double? Deserved { get; set; } // For workmanship certificates
    public double? Insurance { get; set; } // For workmanship certificates
    public double? AdditionalInsurance { get; set; } // For workmanship certificates
    public double? Net { get; set; } // For workmanship certificates
    public string AchievementPercentage { get; set; } // For workmanship certificates
}