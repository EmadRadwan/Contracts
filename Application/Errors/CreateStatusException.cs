namespace Application.Errors;

public class CreateStatusException : Exception
{
    public CreateStatusException(string message) : base(message)
    {
    }

    public CreateStatusException(string message, Exception innerException) : base(message, innerException)
    {
    }
}