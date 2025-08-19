using System.ComponentModel.DataAnnotations;

namespace Application.Shipments.FinAccounts;

public class FinAccountRecord
{
    [Key] public string FinAccountId { get; set; } = null!;

    public string? FinAccountTypeId { get; set; }
    public string? FinAccountTypeDescription { get; set; }
    public string? StatusId { get; set; }
    public string? FinAccountName { get; set; }
    public string? FinAccountCode { get; set; }
    public string? FinAccountPin { get; set; }
    public string? CurrencyUomId { get; set; }
    public string? CurrencyUomDescription { get; set; }
    public string? OrganizationPartyId { get; set; }
    public string? OrganizationPartyName { get; set; }
    public string? OwnerPartyId { get; set; }
    public string? OwnerPartyName { get; set; }
    public string? PostToGlAccountId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public string? IsRefundable { get; set; }
    public string? ReplenishPaymentId { get; set; }
    public decimal? ReplenishLevel { get; set; }
    public decimal? ActualBalance { get; set; }
    public decimal? AvailableBalance { get; set; }
}