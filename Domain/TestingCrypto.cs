namespace Domain;

public class TestingCrypto
{
    public string TestingCryptoId { get; set; } = null!;
    public string? TestingCryptoTypeId { get; set; }
    public string? UnencryptedValue { get; set; }
    public string? EncryptedValue { get; set; }
    public string? SaltedEncryptedValue { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }
}