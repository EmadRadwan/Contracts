namespace Domain;

public class TestFieldType
{
    public string TestFieldTypeId { get; set; } = null!;
    public byte[]? BlobField { get; set; }
    public byte[]? ByteArrayField { get; set; }
    public byte[]? ObjectField { get; set; }
    public DateTime? DateField { get; set; }
    public DateTime? TimeField { get; set; }
    public DateTime? DateTimeField { get; set; }
    public decimal? FixedPointField { get; set; }
    public double? FloatingPointField { get; set; }
    public int? NumericField { get; set; }
    public string? ClobField { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }
}