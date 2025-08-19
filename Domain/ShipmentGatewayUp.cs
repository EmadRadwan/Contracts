namespace Domain;

public class ShipmentGatewayUp
{
    public string ShipmentGatewayConfigId { get; set; } = null!;
    public string? ConnectUrl { get; set; }
    public int? ConnectTimeout { get; set; }
    public string? ShipperNumber { get; set; }
    public string? BillShipperAccountNumber { get; set; }
    public string? AccessLicenseNumber { get; set; }
    public string? AccessUserId { get; set; }
    public string? AccessPassword { get; set; }
    public string? SaveCertInfo { get; set; }
    public string? SaveCertPath { get; set; }
    public string? ShipperPickupType { get; set; }
    public string? CustomerClassification { get; set; }
    public decimal? MaxEstimateWeight { get; set; }
    public decimal? MinEstimateWeight { get; set; }
    public string? CodAllowCod { get; set; }
    public decimal? CodSurchargeAmount { get; set; }
    public string? CodSurchargeCurrencyUomId { get; set; }
    public string? CodSurchargeApplyToPackage { get; set; }
    public string? CodFundsCode { get; set; }
    public string? DefaultReturnLabelMemo { get; set; }
    public string? DefaultReturnLabelSubject { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ShipmentGatewayConfig ShipmentGatewayConfig { get; set; } = null!;
}