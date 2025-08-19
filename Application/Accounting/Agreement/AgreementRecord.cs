using System.ComponentModel.DataAnnotations;

namespace Application.Shipments.Agreement;

public class AgreementRecord
{
    [Key] public string AgreementId { get; set; } = null!;

    public string? PartyIdFrom { get; set; }
    public string? PartyIdFromName { get; set; }
    public string? PartyIdTo { get; set; }
    public string? PartyIdToName { get; set; }
    public string? RoleTypeIdFrom { get; set; }
    public string? RoleTypeIdFromDescription { get; set; }
    public string? RoleTypeIdTo { get; set; }
    public string? RoleTypeIdToDescription { get; set; }
    public string? AgreementTypeId { get; set; }
    public string? AgreementTypeIdDescription { get; set; }
    public DateTime? AgreementDate { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public string? Description { get; set; }
    public string? TextData { get; set; }
    public string? StatusId { get; set; }
}