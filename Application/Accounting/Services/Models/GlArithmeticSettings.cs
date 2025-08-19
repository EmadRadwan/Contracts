namespace Application.Accounting.Services.Models;

public class GlArithmeticSettings
{
    public decimal DecimalScale { get; set; }
    public string RoundingMode { get; set; }
}

public enum RoundingMode
{
    HalfUp,
    HalfDown,
    ToEven
    // Add other modes as necessary
}