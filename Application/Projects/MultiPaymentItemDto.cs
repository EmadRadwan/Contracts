namespace Application.Projects;

public class MultiPaymentItemDto
{
    public string WorkEffortId { get; set; }
    public string ProjectId { get; set; }
    public string ProjectName { get; set; }
    public string SubProjectId { get; set; }
    public string SubProjectName { get; set; }
    public string ItemType { get; set; }
    public string ItemTypeDescription { get; set; }
    public string ServiceId { get; set; }
    public string ServiceName { get; set; }
    public string ProductId { get; set; }
    public string ProductName { get; set; }
    public string Description { get; set; }
    public decimal? Amount { get; set; }
    public decimal? Discount { get; set; }
    public string DiscountMode { get; set; }
    public decimal? TransportationExpenses { get; set; }
    public decimal? Gratuities { get; set; }
    public decimal? Total { get; set; }
    public string PartyIdSupplier { get; set; }
    public string PartyIdSupplierName { get; set; }
    public string PartyIdContractor { get; set; }
    public string PartyIdContractorName { get; set; }
}