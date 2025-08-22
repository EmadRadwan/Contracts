using System.ComponentModel.DataAnnotations;
using Application.Catalog.Products;

namespace Application.Manufacturing;

public class WorkEffortRecord
{
    [Key]
    public string WorkEffortId { get; set; } = null!;
    public string? WorkEffortTypeId { get; set; }
    public string? CurrentStatusId { get; set; }
    public string? CurrentStatusDescription { get; set; }
    public string? StatusDescription { get; set; }
    public DateTime? LastStatusUpdate { get; set; }
    public string? WorkEffortPurposeTypeId { get; set; }
    public string? WorkEffortPurposeTypeDescription { get; set; }
    public string? WorkEffortParentId { get; set; }
    public string? ScopeEnumId { get; set; }
    public int? Priority { get; set; }
    public int? PercentComplete { get; set; }
    public string? WorkEffortName { get; set; }
    public string? ShowAsEnumId { get; set; }
    public string? SendNotificationEmail { get; set; }
    public string? Description { get; set; }
    public string? LocationDesc { get; set; }
    public DateTime? EstimatedStartDate { get; set; }
    public DateTime? EstimatedCompletionDate { get; set; }
    public DateTime? ActualStartDate { get; set; }
    public DateTime? ActualCompletionDate { get; set; }
    public double? EstimatedMilliSeconds { get; set; }
    public double? EstimatedSetupMillis { get; set; }
    public string? EstimateCalcMethod { get; set; }
    public double? ActualMilliSeconds { get; set; }
    public double? ActualSetupMillis { get; set; }
    public double? TotalMilliSecondsAllowed { get; set; }
    public decimal? TotalMoneyAllowed { get; set; }
    public string? MoneyUomId { get; set; }
    public string? SpecialTerms { get; set; }
    public int? TimeTransparency { get; set; }
    public string? UniversalId { get; set; }
    public string? SourceReferenceId { get; set; }
    public string? FixedAssetId { get; set; }
    public string? FixedAssetName { get; set; }
    public ProductLovDto? ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? FacilityId { get; set; }
    public string? FacilityName { get; set; }
    public string? InfoUrl { get; set; }
    public string? RecurrenceInfoId { get; set; }
    public string? TempExprId { get; set; }
    public string? RuntimeDataId { get; set; }
    public string? NoteId { get; set; }
    public string? ServiceLoaderName { get; set; }
    public decimal? QuantityToProduce { get; set; }
    public string UomAndQuantity { get; set; }
    public decimal? QuantityProduced { get; set; }
    public decimal? QuantityRejected { get; set; }
    public decimal? ReservPersons { get; set; }
    public decimal? Reserv2ndPPPerc { get; set; }
    public decimal? ReservNthPPPerc { get; set; }
    public string? AccommodationMapId { get; set; }
    public string? AccommodationSpotId { get; set; }
    public string? ProjectNum { get; set; }
    public string? ProjectName { get; set; }
    public int? RevisionNumber { get; set; }
}