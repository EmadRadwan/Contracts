using System.ComponentModel.DataAnnotations;

namespace Application.Manufacturing;

public class WorkEffortDto
{
    public string WorkEffortId { get; set; } = null!;
    public string? WorkEffortTypeId { get; set; }
    public string? CurrentStatusId { get; set; }
    public string? CurrentStatusDescription { get; set; }
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
    public int? SequenceNum { get; set; }

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
    public string? FacilityId { get; set; }
    public string? InfoUrl { get; set; }
    public string? RecurrenceInfoId { get; set; }
    public string? TempExprId { get; set; }
    public string? RuntimeDataId { get; set; }
    public string? NoteId { get; set; }
    public string? ServiceLoaderName { get; set; }
    public decimal? QuantityToProduce { get; set; }
    public decimal? QuantityProduced { get; set; }
    public decimal? QuantityRejected { get; set; }
    public decimal? ReservPersons { get; set; }
    public decimal? Reserv2ndPPPerc { get; set; }
    public decimal? ReservNthPPPerc { get; set; }
    public string? AccommodationMapId { get; set; }
    public string? AccommodationSpotId { get; set; }
    public string? CanDeclareAndProduce { get; set; }
    public string? CanProduce { get; set; }
    public string? LastLotId { get; set; }
    public int? RevisionNumber { get; set; }
    public bool IsStartTask { get; set; }
    public bool IsIssueTask { get; set; }
    public bool IsCompleteTask { get; set; }
    public bool IsFinalTask { get; set; }
}