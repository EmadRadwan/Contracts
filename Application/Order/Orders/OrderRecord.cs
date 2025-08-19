using System.ComponentModel.DataAnnotations;
using Application.Services;

namespace Application.Order.Orders;

public class OrderRecord
{
    [Key] // Decorate the key property with the Key attribute
    public string OrderId { get; set; }

    public string? OrderTypeId { get; set; }
    public string? OrderTypeDescription { get; set; }

    public string? PaymentMethodId { get; set; }
    public string? PaymentMethodTypeId { get; set; }
    public string? AgreementId { get; set; }
    public string? PaymentId { get; set; }
    public string? InvoiceId { get; set; }

    public OrderPartyDto? FromPartyId { get; set; }
    public string? FromPartyName { get; set; }
    public DateTime? OrderDate { get; set; }
    public string? StatusId { get; set; }
    public string? CurrencyUomId { get; set; }
    public string? CurrencyUomDescription { get; set; }
    public string? ProductStoreId { get; set; }
    public string? SalesChannelEnumId { get; set; }
    public decimal? GrandTotal { get; set; }
    public DateTime LastUpdatedStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public decimal? TotalAdjustments { get; set; }
    public string? StatusDescription { get; set; }
    public VehicleLovDto? VehicleId { get; set; }

    public string? ChassisNumber { get; set; }

    public string? CustomerRemarks { get; set; }
    public string? InternalRemarks { get; set; }
    public int? CurrentMileage { get; set; }

    public bool AllowSubmit { get; set; }
    public string? BillingAccountId { get; set; }
    public decimal? UseUpToFromBillingAccount { get; set; }
    public ICollection<OrderItemDto>? OrderItems { get; set; }
    public ICollection<OrderAdjustmentDto>? OrderAdjustments { get; set; }
}