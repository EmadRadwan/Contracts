namespace Application.Errors;

public class CreateShipmentException : Exception
{
    public CreateShipmentException(string message) : base(message)
    {
    }

    public CreateShipmentException(string message, Exception innerException) : base(message, innerException)
    {
    }
}